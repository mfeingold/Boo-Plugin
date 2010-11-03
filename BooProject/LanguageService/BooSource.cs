using System;
using System.IO;
using System.Runtime.InteropServices;
using antlr;
using Boo.Lang.Compiler;
using Boo.Lang.Parser;
using Hill30.BooProject.LanguageService.NodeMapping;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using Hill30.BooProject.Project;

namespace Hill30.BooProject.LanguageService
{
    public class BooSource : Source
    {
        private CompilerContext compileResult;
        private readonly Service service;
        public Mapper Mapper { get; private set; }
        private readonly ITextBuffer buffer;
        private readonly IProjectManager projectManager;

        public BooSource(Service service, IVsTextLines buffer, Microsoft.VisualStudio.Package.Colorizer colorizer)
            : base(service, buffer, colorizer)
        {
            this.service = service;
            this.buffer = service.BufferAdapterService.GetDataBuffer(buffer);
            var hierarchyItem = new RunningDocumentTable(this.service.Site).GetHierarchyItem(GetFilePath());
            object value;
            ErrorHandler.ThrowOnFailure(hierarchyItem.GetProperty(VSConstants.VSITEMID_ROOT, (int) __VSHPROPID.VSHPROPID_Root, out value));
            var pointer = new IntPtr((int)value);
            try
            {
                projectManager = Marshal.GetObjectForIUnknown(pointer) as IProjectManager;
            }
            finally
            {
                Marshal.Release(pointer);
            }
        }

        private BooCompiler compiler;

        internal void Compile(ParseRequest req)
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
                    if (compiler == null)
                    {
                        compiler = projectManager.CreateCompiler();
                        compiler.Parameters.Pipeline.AfterStep += Pipeline_AfterStep;
                    }
                    compileResult = compiler.Run(BooParser.ParseReader(service.GetLanguagePreferences().TabSize, "code", new StringReader(req.Text)));
//                    new AstWalker(Mapper).Visit(compileResult.CompileUnit);
                }
                catch
                {}
                Mapper.Complete();
            }
            if (Recompiled != null)
                Recompiled(this, EventArgs.Empty);
        }

        void Pipeline_AfterStep(object sender, CompilerStepEventArgs args)
        {
            if (args.Step == ((CompilerPipeline)sender)[0])
                new AstWalker(Mapper).Visit(args.Context.CompileUnit);
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
                               {iStartLine = line, iStartIndex = node.StartPos, iEndLine = line, iEndIndex = node.EndPos};
                    return node.QuickInfoTip;

                }
            }
            span = new TextSpan { iStartLine = line, iStartIndex = col, iEndLine = line, iEndIndex = col };
            return "";
        }

    }
}
