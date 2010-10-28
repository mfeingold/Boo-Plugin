using System;
using System.IO;
using antlr;
using Boo.Lang.Compiler;
using Boo.Lang.Parser;
using Hill30.BooProject.LanguageService.NodeMapping;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.BooProject.LanguageService
{
    public class BooSource : Source
    {
        private CompilerContext compileResult;
        private readonly Service service;
        public Mapper Mapper { get; private set; }
        private readonly ITextBuffer buffer;

        public BooSource(Service service, IVsTextLines buffer, Microsoft.VisualStudio.Package.Colorizer colorizer)
            : base(service, buffer, colorizer)
        {
            this.service = service;
            this.buffer = service.BufferAdapterService.GetDataBuffer((IVsTextBuffer) buffer);
        }

        internal void Compile(BooCompiler compiler, ParseRequest req)
        {
            lock (this)
            {
                Mapper = new Mapper(service.ClassificationTypeRegistry, buffer, service.GetLanguagePreferences().TabSize);
                var lexer = BooParser.CreateBooLexer(service.GetLanguagePreferences().TabSize, "code stream", new StringReader(req.Text));
                try
                {
                    IToken token;
                    while ((token = lexer.nextToken()).Type != BooLexer.EOF)
                        Mapper.MapToken(token);

                    compileResult = compiler.Run(BooParser.ParseReader(service.GetLanguagePreferences().TabSize, "code", new StringReader(req.Text)));
                    new AstWalker(this, Mapper).Visit(compileResult.CompileUnit);
                }
                catch
                {}
                Mapper.CompleteComments();
            }
            if (Recompiled != null)
                Recompiled(this, EventArgs.Empty);
        }

        public event EventHandler Recompiled;

        internal string GetDataTipText(int line, int col, out TextSpan span)
        {
            if (Mapper != null)
            {
                var node = Mapper.GetNode(line, col);
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

    }
}
