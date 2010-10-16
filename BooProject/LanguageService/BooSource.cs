using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using antlr;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Parser;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.BooProject.LanguageService
{
    public class BooSource : Source
    {
        public BooSource(Service service, IVsTextLines buffer, Colorizer colorizer)
            : base(service, buffer, colorizer)
        { }

        private CompilerContext compileResult;

        public CompilerContext CompileResult
        {
            get
            {
                lock (this)
                {
                    return compileResult;
                }
            }
        }

        private readonly Dictionary<int, List<Node>> types = new Dictionary<int, List<Node>>();

        private class AstWalker : DepthFirstVisitor
        {
            public AstWalker(BooSource source)
            {
                this.source = source;
            }

            private readonly BooSource source;

            public override void OnClassDefinition(ClassDefinition node)
            {
                RegisterType(node);
                base.OnClassDefinition(node);
            }

            private void RegisterType(Node node)
            {
                List<Node> list;
                if (!source.types.TryGetValue(node.LexicalInfo.Line, out list))
                    source.types[node.LexicalInfo.Line] = list = new List<Node>();
                list.Add(node);
            }

            public override void OnSimpleTypeReference(SimpleTypeReference node)
            {
                RegisterType(node);
                base.OnSimpleTypeReference(node);
            }

            protected override void OnError(Node node, Exception error)
            {
                // Do Nothing
            }
        }

        //internal IEnumerable<Node> GetNodes(int line, int pos)
        //{
        //    foreach (var node in nodes)
        //    {
        //        if (node.LexicalInfo == null) continue;
        //        if (node.LexicalInfo.Line > line) continue;
        //        if (node.LexicalInfo.Line == line && node.LexicalInfo.Column > pos) continue;
        //        if (node.EndSourceLocation.Line < line) continue;
        //        if (node.EndSourceLocation.Line == line && node.EndSourceLocation.Column < pos) continue;
        //        yield return node;
        //    }
        //}

        private readonly List<IToken> tokens = new List<IToken>();

        internal void Compile(BooCompiler compiler, ParseRequest req)
        {
            lock (this)
            {
                tokens.Clear();
                var lexer = BooParser.CreateBooLexer(1, "code stream", new StringReader(req.Text));
                try
                {
                    IToken token;
                    while ((token = lexer.nextToken()).Type != BooLexer.EOF)
                        tokens.Add(token);
                }
                catch (Exception)
                {}
                compileResult = compiler.Run(BooParser.ParseString("code", req.Text));

                types.Clear();
                new AstWalker(this).Visit(compileResult.CompileUnit);
            }
            Recolorize(0, GetLineCount()-1);
        }

        internal string GetDataTipText(int line, int col, out TextSpan span)
        {
            //foreach (var node in GetNodes(line + 1, col + 1))
            //{
            //    if (node.NodeType == NodeType.CallableTypeReference)
            //        break;
            //}
            span = new TextSpan { iStartLine = line, iStartIndex = col, iEndLine = line, iEndIndex = col + 10 };

            return "";
        }

        internal bool IsBlockComment(int line, IToken token)
        {
            lock (this)
            {
                if (compileResult == null)
                    return false; // do not know yet
                foreach (var t in tokens)
                    if (t.getColumn() == token.getColumn()
                        && t.getLine() == line
                        && t.getText() == token.getText()
                        )
                        return false;
                return true;
            }
        }

        internal TokenColor GetColorForID(int line, IToken token)
        {
            lock (this)
            {
                if (compileResult == null)
                    return TokenColor.Identifier; // do not know yet
                List<Node> nodes;
                if (types.TryGetValue(line, out nodes))
                    if (nodes.Any(node => node.LexicalInfo.Column == token.getColumn()))
                        return (TokenColor) BooColorizer.TokenColor.Type;

                return TokenColor.Identifier;
            }
        }
    }
}
