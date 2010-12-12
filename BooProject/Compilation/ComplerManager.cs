using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Reflection;
using Boo.Lang.Compiler;
using Boo.Lang.Parser;
using Hill30.BooProject.AST;
using Hill30.BooProject.Project;
using Microsoft.VisualStudio.Shell.Design;
using VSLangProj;
using Microsoft.VisualStudio.Project.Automation;

namespace Hill30.BooProject.Compilation
{
    public class ComplerManager
    {
        private readonly BooCompiler compiler;
        private readonly List<BooFileNode> compileList = new List<BooFileNode>();
        private readonly BooProjectNode projectManager;
        private DynamicTypeService.ContextTypeResolver resolverContext;
        private ITypeResolutionService typeResolver;
        private readonly List<Assembly> references = new List<Assembly>();
        private bool initialized;

        public ComplerManager(BooProjectNode projectManager)
        {
            this.projectManager = projectManager;
            var pipeline = CompilerPipeline.GetPipeline("compile");
            pipeline.BreakOnErrors = false;
            compiler = new BooCompiler(new CompilerParameters(false) { Pipeline = pipeline });
        }

        private void Initialize()
        {
            resolverContext = GlobalServices.TypeService.GetContextTypeResolver(projectManager);
            typeResolver = GlobalServices.TypeService.GetTypeResolutionService(projectManager);
            foreach (Reference reference in projectManager.VSProject.References)
            {
                switch (reference.Type)
                {
                    case prjReferenceType.prjReferenceTypeAssembly:
                        if (!File.Exists(reference.Path))
                            break;
                        references.Add(typeResolver.GetAssembly(new AssemblyName(reference.Path)));
                        break;
                    default:
                        break;
                }
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
            lock (compileList)
            {
                if (!initialized)
                {
                    //Initialize();
                    initialized = true;
                }
                localCompileList = new List<BooFileNode>(compileList);
                compileList.Clear();
            }
            if (localCompileList.Count == 0)
                return;

            ((BooParsingStep)compiler.Parameters.Pipeline[0]).TabSize = GlobalServices.LanguageService.GetLanguagePreferences().TabSize;

            compiler.Parameters.Input.Clear();
            compiler.Parameters.References.Clear();
            references.ForEach(a => compiler.Parameters.References.Add(a));

            var results = new Dictionary<string, Tuple<BooFileNode, CompileResults>>();
            foreach (var file in BooProjectNode.GetFileEnumerator(projectManager))
                if (localCompileList.Contains(file))
                {
                    var result = new CompileResults(file);
                    var input = file.GetCompilerInput(result);
                    results.Add(input.Name, new Tuple<BooFileNode, CompileResults>(file, result));
                    compiler.Parameters.Input.Add(input);
                }
                else
                    compiler.Parameters.References.Add(file.CompileUnit);

            CompilerStepEventHandler handler =
                (sender, args) =>
                {
                    if (args.Step == ((CompilerPipeline)sender)[0])
                        CompileResults.MapParsedNodes(results, args.Context);
                };

            compiler.Parameters.Pipeline.AfterStep += handler;
            CompileResults.MapCompleted(results, compiler.Run());
            compiler.Parameters.Pipeline.AfterStep -= handler;
            foreach (var item in results.Values)
                item.Item1.SetCompilerResults(item.Item2);
        }

        public void Dispose()
        {
            ((IDisposable)resolverContext).Dispose();
        }
    }
}
