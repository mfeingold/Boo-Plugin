using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Boo.Lang.Compiler;
using Hill30.BooProject.AST;
using Hill30.BooProject.Project;

namespace Hill30.BooProject.Compilation
{
    public class ComplerManager
    {
        private readonly BooCompiler compiler;

        private readonly List<BooFileNode> compileList = new List<BooFileNode>();
        private readonly BooProjectNode projectManager;

        public ComplerManager(BooProjectNode projectManager)
        {
            this.projectManager = projectManager;
            var pipeline = CompilerPipeline.GetPipeline("compile");
            pipeline.BreakOnErrors = false;
            compiler = new BooCompiler(new CompilerParameters(true) { Pipeline = pipeline });
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
                localCompileList = new List<BooFileNode>(compileList);
                compileList.Clear();
            }
            if (localCompileList.Count == 0)
                return;

            compiler.Parameters.Input.Clear();
            compiler.Parameters.References.Clear();
            compiler.Parameters.References.Add(Assembly.GetAssembly(typeof(ArrayList)));

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
            
        }

    }
}
