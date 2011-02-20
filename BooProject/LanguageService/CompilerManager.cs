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
using System.Linq;
using System.Reflection;
using Hill30.BooProject.Project;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Design;
using Hill30.Boo.ASTMapper;

namespace Hill30.BooProject.Compilation
{
    public class CompilerManager
    {
        private readonly List<BooFileNode> compileList = new List<BooFileNode>();
        private readonly BooProjectNode projectManager;
        private readonly Dictionary<uint, AssemblyEntry> references = new Dictionary<uint, AssemblyEntry>();
        private IDisposable typeResolverContext;
        private ITypeResolutionService typeResolver;
        private bool referencesDirty;

        public CompilerManager(BooProjectNode projectManager)
        {
            this.projectManager = projectManager;
            references.Add((uint)VSConstants.VSITEMID.Root, new AssemblyEntry(new AssemblyName("mscorlib")));
        }

        public void Initialize()
        {
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
            references.Add(referenceNode.ID, new AssemblyEntry(referenceNode));
            UpdateReferences();
        }

        class AssemblyEntry
        {
            private readonly ReferenceNode reference;
            private Assembly assembly;
            readonly AssemblyName assemblyName;

            public AssemblyEntry(AssemblyName assemblyName)
            {
                this.assemblyName = assemblyName;
            }

            public AssemblyEntry(ReferenceNode reference)
            {
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
// ReSharper disable AssignNullToNotNullAttribute
                        return new AssemblyName(Path.GetFileNameWithoutExtension(assemblyNameProperty.Value.ToString()));
// ReSharper restore AssignNullToNotNullAttribute
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
            
            public Assembly GetAssembly(Func<AssemblyName, Assembly> assemblyResolver)
            { 
                if (assembly == null)
                {
                    var aName = assemblyName ?? GetAssemblyName();
                    if (aName != null)
                        assembly = assemblyResolver(aName);
                }
                return assembly;
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
                if (typeResolverContext == null)
                {
                    typeResolverContext = GlobalServices.TypeService.GetContextTypeResolver(projectManager);
                    typeResolver = GlobalServices.TypeService.GetTypeResolutionService(projectManager);
                }
                localCompileList = new List<BooFileNode>(compileList);
                compileList.Clear();
                if (localCompileList.Count == 0 && !referencesDirty)
                    return;
                recompileAll = referencesDirty;
                referencesDirty = false;
            }

            var results = new Dictionary<BooFileNode, CompileResults>();
            foreach (var file in BooProjectNode.GetFileEnumerator(projectManager))
                if (recompileAll || localCompileList.Contains(file))
                {
                    // this seemingly redundant variable ensures that each closure below has its own copy of
                    // the file reference. Without it they share the same copy decalred in the loop statemenet
                    // essentially all of them will point to the last element in the loop
                    var localfile = file;
                    results.Add(file, new CompileResults(() => localfile.Url, localfile.GetCompilerInput, ()=>GlobalServices.LanguageService.GetLanguagePreferences().TabSize));
                }
                else
                    results.Add(file, file.GetCompileResults());

            Boo.ASTMapper.CompilerManager.Compile(references.Values.Select(ae => ae.GetAssembly(typeResolver.GetAssembly)).Where(a => a != null), results.Values);

            foreach (var result in results)
                result.Key.SetCompilerResults(result.Value);

        }

        public void Dispose()
        {
            if (typeResolverContext != null)
                typeResolverContext.Dispose();
            GlobalServices.TypeService.AssemblyRefreshed -= AssemblyRefreshed;
            GlobalServices.TypeService.AssemblyObsolete -= AssemblyObsolete;
            GlobalServices.TypeService.AssemblyDeleted -= AssemblyDeleted;
        }

    }
}
