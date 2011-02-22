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
using System.Reflection;
using Boo.Lang.Compiler;

namespace Hill30.Boo.ASTMapper
{
    public static class CompilerManager
    {
        public static void Compile(IEnumerable<Assembly> assemblies, IEnumerable<CompileResults> codeFiles)
        {

            var pipeline = CompilerPipeline.GetPipeline("compile");
            pipeline.BreakOnErrors = false;
            var compiler = new BooCompiler(new CompilerParameters(false) { Pipeline = pipeline });

            //            ((BooParsingStep)compiler.Parameters.Pipeline[0]).TabSize = GlobalServices.LanguageService.GetLanguagePreferences().TabSize;

            compiler.Parameters.Input.Clear();
            compiler.Parameters.References.Clear();

            foreach (var assembly in assemblies)
                compiler.Parameters.References.Add(assembly);

            var results = new Dictionary<string, CompileResults>();

            foreach (var codeFile in codeFiles)
            {
                results.Add(codeFile.Url, codeFile);
                codeFile.SetupForCompilation(compiler.Parameters);
            }

            compiler.Parameters.Pipeline.AfterStep +=
                (sender, args) =>
                {
                    if (args.Step == pipeline[0])
                        MapParsedNodes(results, args.Context);
                    if (args.Step == pipeline[pipeline.Count - 1])
                        MapCompleted(results, args.Context);
                };

            // as a part of compilation process compiler might request assembly load which triggers an assembly 
            // resolve event to be processed by type resolver. Such processing has to happen on the same thread the
            // resolver has been created on
            compiler.Run();

        }

        public static void MapParsedNodes(Dictionary<string, CompileResults> results, CompilerContext compilerContext)
        {
            foreach (var module in compilerContext.CompileUnit.Modules)
                results[module.LexicalInfo.FileName].MapParsedNodes(module);

            foreach (var error in compilerContext.Errors)
                results[error.LexicalInfo.FileName].MapParsingMessage(error);

            foreach (var warning in compilerContext.Warnings)
                results[warning.LexicalInfo.FileName].MapParsingMessage(warning);
        }

        public static void MapCompleted(Dictionary<string, CompileResults> results, CompilerContext compilerContext)
        {
            foreach (var module in compilerContext.CompileUnit.Modules)
                if (module.LexicalInfo.FullPath != null)
                    results[module.LexicalInfo.FileName].MapCompletedNodes(module);

            foreach (var error in compilerContext.Errors)
                results[error.LexicalInfo.FileName].MapMessage(error);

            foreach (var warning in compilerContext.Warnings)
                results[warning.LexicalInfo.FileName].MapMessage(warning);
        }

    }
}
