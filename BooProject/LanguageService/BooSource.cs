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
        { this.service = service; }

        private CompilerContext compileResult;
        private Service service;

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
            private readonly Action<Node, int> registerType;
            public AstWalker(BooSource source, Action<Node, int> registerType)
            {
                this.source = source;
                this.registerType = registerType;
            }

            private readonly BooSource source;

            public override void OnClassDefinition(ClassDefinition node)
            {
                registerType(node, node.Name.Length);
                base.OnClassDefinition(node);
            }

            public override void OnSimpleTypeReference(SimpleTypeReference node)
            {
                registerType(node, node.Name.Length);
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

        private TokenMapper mapper;

        internal void Compile(BooCompiler compiler, ParseRequest req)
        {
            lock (this)
            {
                mapper = new TokenMapper(service.GetLanguagePreferences().TabSize, req.Text);
                tokens.Clear();
                types.Clear();
                var lexer = BooParser.CreateBooLexer(service.GetLanguagePreferences().TabSize, "code stream", new StringReader(req.Text));
                try
                {
                    IToken token;
                    while ((token = lexer.nextToken()).Type != BooLexer.EOF)
                        tokens.Add(token);
                    compileResult = compiler.Run(BooParser.ParseReader(service.GetLanguagePreferences().TabSize, "code", new StringReader(req.Text)));
                    new AstWalker(this,
                        (Node node, int length) => {                
                                List<Node> list;
                                if (!types.TryGetValue(node.LexicalInfo.Line, out list))
                                    types[node.LexicalInfo.Line] = list = new List<Node>();
                                list.Add(node);
                                mapper.MapNode(node, length);
                            }
                        ).Visit(compileResult.CompileUnit);
                }
                catch (Exception)
                {}
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
            var node = mapper.GetNode(line, col);
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
