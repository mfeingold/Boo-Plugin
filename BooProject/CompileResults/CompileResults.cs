using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Boo.Lang.Compiler;
using Boo.Lang.Parser;
using Hill30.BooProject.CachedAST;
using Hill30.BooProject.LanguageService;
using Boo.Lang.Compiler.IO;
using Microsoft.VisualStudio.Shell;
using CompilerParameters = Boo.Lang.Compiler.CompilerParameters;

namespace Hill30.BooProject.CompileResults
{
    /// <summary>
    /// The cached AST for a Boo source file
    /// </summary>
    public class CompileResults
    {
        private readonly List<ASTAnchor> astAnchors = new List<ASTAnchor>();
        private int[][] positionMap;
        private int lineSize;

        private static antlr.IToken NextToken(antlr.TokenStream tokens)
        {
            while (true)
                try { return tokens.nextToken(); }
                // ReSharper disable EmptyGeneralCatchClause
                catch { }
                // ReSharper restore EmptyGeneralCatchClause
        }

        private void ExpandTabs(string source, int tabSize)
        {
            var sourcePos = 0;
            var mappedPos = 0;
            var mappings = new List<int[]>();
            var positionList = new List<int>();
            lineSize = 0;
            foreach (var c in source)
            {
                if (c == '\t')
                    while (mappedPos % tabSize < tabSize - 1)
                    {
                        positionList.Add(sourcePos);
                        mappedPos++;
                    }
                positionList.Add(sourcePos++);
                mappedPos++;
                if (c == '\n')
                {
                    lineSize = Math.Max(lineSize, mappedPos);
                    mappings.Add(positionList.ToArray());
                    positionList.Clear();
                    sourcePos = 0;
                    mappedPos = 0;
                }
            }
            positionList.Add(sourcePos); // to map the <EOL> token
            mappings.Add(positionList.ToArray());
            positionMap = mappings.ToArray();
        }

        public void Initialize(string filePath, string source)
        {
            var tabSize = GlobalServices.LanguageService.GetLanguagePreferences().TabSize;
            ExpandTabs(source, tabSize);
            astAnchors.Clear();
            MapTokens(tabSize, source);
        }

        private void MapTokens(int tabSize, string source)
        {

            var tokens = BooParser.CreateBooLexer(tabSize, "code stream", new StringReader(source));

            antlr.IToken token;
            var currentPos = 0;

            while ((token = NextToken(tokens)).Type != BooLexer.EOF)
            {
                int length;

                switch (token.Type)
                {
                    case BooLexer.INDENT:
                    case BooLexer.DEDENT:
                    case BooLexer.EOL:
                        continue;
                    case BooLexer.SINGLE_QUOTED_STRING:
                    case BooLexer.DOUBLE_QUOTED_STRING:
                        length = token.getText().Length + 2;
                        break;
                    case BooLexer.TRIPLE_QUOTED_STRING:
                        length = token.getText().Length + 6;
                        break;
                    default:
                        length = token.getText().Length;
                        break;
                }

                var startIndex = positionMap[token.getLine() - 1][token.getColumn() - 1];
                var endIndex = positionMap[token.getLine() - 1][token.getColumn() - 1 + length];
                var startLine = token.getLine() - 1;

                var cluster = new ASTAnchor(
                    startLine * lineSize + startIndex,
                    endIndex - startIndex,
                    token);

                if (astAnchors.Count > 0
                    && astAnchors[astAnchors.Count() - 1].Index >= cluster.Index)
                    throw new ArgumentException("Token Mapping order");
                astAnchors.Add(cluster);

//                currentPos = tokenHandler(currentPos, token);
            }

//            tokenHandler(currentPos, token);
        }

        private void Compile(int tabSize, string filePath, string source)
        {
            var pipeline = CompilerPipeline.GetPipeline("compile");
            pipeline.BreakOnErrors = false;
            var compiler = new BooCompiler(new CompilerParameters(true) { Pipeline = pipeline });
                
            compiler.Parameters.Pipeline.AfterStep += PipelineAfterStep;
            
            var parsingStep = (BooParsingStep)compiler.Parameters.Pipeline.Get(typeof(BooParsingStep));
            parsingStep.TabSize = tabSize;
            compiler.Parameters.Input.Clear();
            compiler.Parameters.Input.Add(new StringInput(filePath, source));
            var compileResult = compiler.Run();
            //Complete(compileResult);

        }

        private void PipelineAfterStep(object sender, CompilerStepEventArgs args)
        {
            //if (args.Step == ((CompilerPipeline)sender)[0])
            //    MapParsedNodes(args.Context);
        }

        public TService GetService<TService>()
        {
            return (TService)Package.GetGlobalService(typeof(TService));
        }

    }
}
