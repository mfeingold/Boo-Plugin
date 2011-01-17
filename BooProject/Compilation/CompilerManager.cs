//
//   Copyright © 2010 Michael Feingold
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Reflection;
using Boo.Lang.Compiler;
using Boo.Lang.Parser;
using Hill30.BooProject.AST;
using Hill30.BooProject.Project;
using Microsoft.VisualStudio.Project.Automation;
using Microsoft.VisualStudio.Shell.Design;
using VSLangProj;
using Boo.Lang.Compiler.Ast;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Package;

namespace Hill30.BooProject.Compilation
{
    public class CompilerManager
    {
        private readonly List<BooFileNode> compileList = new List<BooFileNode>();
        private readonly BooProjectNode projectManager;
        private DynamicTypeService.ContextTypeResolver resolverContext;
        private ITypeResolutionService typeResolver;
        private Dictionary<uint, AssemblyEntry> references = new Dictionary<uint, AssemblyEntry>();
        private bool referencesDirty;

        public CompilerManager(BooProjectNode projectManager)
        {
            this.projectManager = projectManager;
        }

        public void Initialize()
        {
            resolverContext = GlobalServices.TypeService.GetContextTypeResolver(projectManager);
            typeResolver = GlobalServices.TypeService.GetTypeResolutionService(projectManager);
            GlobalServices.TypeService.AssemblyRefreshed += AssemblyRefreshed;
            GlobalServices.TypeService.AssemblyObsolete += AssemblyObsolete;
            GlobalServices.TypeService.AssemblyDeleted += AssemblyDeleted;
        }

        private void ResetAssemblyReferences(Assembly assembly)
        {
            foreach (var reference in references.Values)
                reference.Refresh(assembly);
            UpdateReferences();
        }

        private void AssemblyRefreshed(object sender, AssemblyRefreshedEventArgs args)
        {
            ResetAssemblyReferences(args.RefreshedAssembly);
        }

        private void AssemblyObsolete(object sender, AssemblyObsoleteEventArgs args)
        {
            ResetAssemblyReferences(args.ObsoleteAssembly);
        }

        void AssemblyDeleted(object sender, AssemblyDeletedEventArgs args)
        {
            ResetAssemblyReferences(args.DeletedAssembly);
        }

        internal void AddReference(ReferenceNode referenceNode)
        {
            references.Add(referenceNode.ID, new AssemblyEntry(this, referenceNode));
            UpdateReferences();
        }

        class AssemblyEntry
        {
            CompilerManager manager;
            ReferenceNode reference;
            Assembly assembly;
            public AssemblyEntry(CompilerManager manager, ReferenceNode reference)
            {
                this.manager = manager;
                this.reference = reference;
            }

            private AssemblyName GetAssemblyName()
            {
                var assemblyReference = reference as AssemblyReferenceNode;
                if (assemblyReference != null)
                    return assemblyReference.AssemblyName;

                var projectReference = reference as ProjectReferenceNode;
                if (projectReference != null)
                    // Now get the name of the assembly from the project.
                    // Some project system throw if the property does not exist. We expect an ArgumentException.
                    try
                    {
                        var assemblyNameProperty = projectReference.ReferencedProjectObject.Properties.Item("OutputFileName");
                        return new AssemblyName(Path.GetFileNameWithoutExtension(assemblyNameProperty.Value.ToString()));
                    }
                    catch (ArgumentException)
                    {
                    }
                return null;
            }

            public void Refresh(Assembly target)
            {
                if (assembly != null && assembly.FullName == target.FullName)
                    assembly = null;// target;
            }
            
            public Assembly Assembly { 
                get 
                {
                    if (assembly == null)
                    {
                        var assemblyName = GetAssemblyName();
                        if (assemblyName != null)
                            // type resolver must always operate on the same thread
                            GlobalServices.LanguageService.Invoke(new Action (() => assembly = manager.typeResolver.GetAssembly(assemblyName)), new object[] {});
                    }
                    return assembly;
                } 
            }
        }

        internal void RemoveReference(ReferenceNode referenceNode)
        {
            references.Remove(referenceNode.ID);
            UpdateReferences();
        }

        private void UpdateReferences()
        {
            var sources = GlobalServices.LanguageService.GetSources();
            lock (compileList)
            {
                lock (sources)
                {
                    foreach (Source source in sources)
                        source.IsDirty = true;
                }
                referencesDirty = true;
            }
        }

        internal void SubmitForCompile(BooFileNode file)
        {
            if (projectManager.IsCodeFile(file.Url) && file.ItemNode.ItemName == "Compile")
                lock (compileList)
                {
                    compileList.Add(file);
                }
        }

        public void Compile()
        {

            List<BooFileNode> localCompileList;
            bool recompileAll;
            lock (compileList)
            {
                localCompileList = new List<BooFileNode>(compileList);
                compileList.Clear();
                if (localCompileList.Count == 0 && !referencesDirty)
                    return;
                recompileAll = referencesDirty;
                referencesDirty = false;
            }

            var pipeline = CompilerPipeline.GetPipeline("compile");
            pipeline.BreakOnErrors = false;
            var compiler = new BooCompiler(new CompilerParameters(false) { Pipeline = pipeline });

            ((BooParsingStep)compiler.Parameters.Pipeline[0]).TabSize = GlobalServices.LanguageService.GetLanguagePreferences().TabSize;

            compiler.Parameters.Input.Clear();
            compiler.Parameters.References.Clear();
            foreach (var a in references.Values)
                if (a.Assembly != null)
                    compiler.Parameters.References.Add(a.Assembly);

            var results = new Dictionary<string, Tuple<BooFileNode, CompileResults>>();
            foreach (var file in BooProjectNode.GetFileEnumerator(projectManager))
                if (recompileAll || localCompileList.Contains(file) || file.CompileUnit == null)
                {
                    var result = new CompileResults(file);
                    var input = file.GetCompilerInput(result);
                    results.Add(input.Name, new Tuple<BooFileNode, CompileResults>(file, result));
                    compiler.Parameters.Input.Add(input);
                }
                else
                    compiler.Parameters.References.Add(file.CompileUnit);

            compiler.Parameters.Pipeline.AfterStep += new CompilerStepEventHandler(
                (sender, args) =>
                {
                    if (args.Step == ((CompilerPipeline)sender)[0])
                        CompileResults.MapParsedNodes(results, args.Context);
                });

            CompilerContext compilerOutput = null;

            // as a part of compilation process compiler might request assembly load which triggers an assembly 
            // resolve event to be processed by type resolver. Such processing has to happen on the same thread the
            // resolver has been created on
            GlobalServices.LanguageService.Invoke(new Action(() => compilerOutput = compiler.Run()), new object[] { });
            CompileResults.MapCompleted(results, compilerOutput);

            foreach (var item in results.Values)
                item.Item1.SetCompilerResults(item.Item2);
        }

        public void Dispose()
        {
            if (resolverContext != null)
                ((IDisposable)resolverContext).Dispose();
            GlobalServices.TypeService.AssemblyRefreshed -= AssemblyRefreshed;
            GlobalServices.TypeService.AssemblyObsolete -= AssemblyObsolete;
            GlobalServices.TypeService.AssemblyDeleted -= AssemblyDeleted;
        }

    }
}
