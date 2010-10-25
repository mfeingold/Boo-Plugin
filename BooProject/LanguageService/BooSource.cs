using System;
using System.Collections.Generic;
using System.IO;
using antlr;
using Boo.Lang.Compiler;
using Boo.Lang.Parser;
using Hill30.BooProject.LanguageService.NodeMapping;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.BooProject.LanguageService
{
    public class BooSource : Source
    {
        private CompilerContext compileResult;
        private Service service;
        private readonly List<IToken> tokens = new List<IToken>();
        private Mapper mapper;

        public BooSource(Service service, IVsTextLines buffer, Colorizer colorizer)
            : base(service, buffer, colorizer)
        { this.service = service; }

        internal void Compile(BooCompiler compiler, ParseRequest req)
        {
            lock (this)
            {
                mapper = new Mapper(service.GetLanguagePreferences().TabSize, req.Text);
                tokens.Clear();
                var lexer = BooParser.CreateBooLexer(service.GetLanguagePreferences().TabSize, "code stream", new StringReader(req.Text));
                try
                {
                    IToken token;
                    while ((token = lexer.nextToken()).Type != BooLexer.EOF)
                        tokens.Add(token);
                    compileResult = compiler.Run(BooParser.ParseReader(service.GetLanguagePreferences().TabSize, "code", new StringReader(req.Text)));
                    new AstWalker(this, mapper).Visit(compileResult.CompileUnit);
                }
                catch (Exception)
                {}
            }
            Recolorize(0, GetLineCount()-1);
        }

        internal string GetDataTipText(int line, int col, out TextSpan span)
        {
            if (mapper != null)
            {
                var node = mapper.GetNode(line, col);
                if (node != null)
                {
                    span = new TextSpan
                               {iStartLine = line, iStartIndex = node.Start, iEndLine = line, iEndIndex = node.End};
                    return node.QuickInfoTip;

                }
            }
            span = new TextSpan { iStartLine = line, iStartIndex = col, iEndLine = line, iEndIndex = col };
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
            if (mapper != null)
            {
                var node = mapper.GetNode(line, token);
                if (node != null)
                    return (TokenColor)node.NodeColor;
            }
            return TokenColor.Identifier;
        }
    }
}
