using System;
using System.Collections.Generic;
using System.Linq;
using antlr;
using Boo.Lang.Parser;
using Microsoft.VisualStudio.Package;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using System.IO;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.BooProject.LanguageService
{
    public class BooSource : Source
    {
        public BooSource(Service service, Microsoft.VisualStudio.TextManager.Interop.IVsTextLines buffer, Colorizer colorizer)
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
                var name = node.BaseTypes;
                base.OnClassDefinition(node);
            }

            public override void OnSimpleTypeReference(SimpleTypeReference node)
            {
                List<Node> list;
                if (!source.types.TryGetValue(node.LexicalInfo.Line, out list))
                    source.types[node.LexicalInfo.Line] = list = new List<Node>();
                list.Add(node);
                base.OnSimpleTypeReference(node);
            }

            public override void OnField(Field node)
            {
                var name = node.Name;
                base.OnField(node);
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

        internal string GetDataTipText(int line, int col, out Microsoft.VisualStudio.TextManager.Interop.TextSpan span)
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
