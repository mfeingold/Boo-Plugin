using System;
using System.IO;
using System.Runtime.InteropServices;
using Boo.Lang.Compiler;
using Boo.Lang.Parser;
using Hill30.BooProject.LanguageService.Mapping;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using Hill30.BooProject.Project;
using Hill30.BooProject.LanguageService.TaskItems;
using Microsoft.VisualStudio.Text.Classification;
using System.Collections.Generic;
using Boo.Lang.Compiler.Ast;

namespace Hill30.BooProject.LanguageService
{
    public class BooSource : Source
    {
        private CompilerContext compileResult;
        private readonly BooLanguageService service;
        private readonly BufferMap bufferMap = new BufferMap();
        private readonly NodeMap nodeMap;
        private readonly ITextBuffer buffer;
        private readonly IProjectManager projectManager;
        private readonly IVsHierarchy hierarchyItem;

        public BooSource(BooLanguageService service, IVsTextLines buffer, Microsoft.VisualStudio.Package.Colorizer colorizer)
            : base(service, buffer, colorizer)
        {
            this.service = service;
            this.buffer = service.BufferAdapterService.GetDataBuffer(buffer);
            bufferMap.Map(this.buffer, service.GetLanguagePreferences().TabSize);
            nodeMap = new NodeMap(service, bufferMap);

// ReSharper disable DoNotCallOverridableMethodsInConstructor
            hierarchyItem = new RunningDocumentTable(this.service.Site).GetHierarchyItem(GetFilePath());
// ReSharper restore DoNotCallOverridableMethodsInConstructor
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
        private Collection errorsMessages;

        internal void Compile(ParseRequest req)
        {
            lock (this)
            {
                bufferMap.Map(buffer, service.GetLanguagePreferences().TabSize);
//                Mapper = new Mapper(service, buffer, service.GetLanguagePreferences().TabSize);
                if (errorsMessages != null)
                    errorsMessages.Dispose();
                errorsMessages = new Collection(projectManager, hierarchyItem);
                var lexer = BooParser.CreateBooLexer(service.GetLanguagePreferences().TabSize, "code stream", new StringReader(req.Text));
                try
                {
                    //IToken token;
                    //while ((token = lexer.nextToken()).Type != BooLexer.EOF)
                    //    Mapper.MapToken(token);
                    nodeMap.Clear();
                    nodeMap.MapTokens(lexer);
                    if (compiler == null)
                    {
                        compiler = projectManager.CreateCompiler();
                        compiler.Parameters.Pipeline.AfterStep += PipelineAfterStep;
                    }
                    compileResult = compiler.Run(BooParser.ParseReader(service.GetLanguagePreferences().TabSize, bufferMap.FilePath, new StringReader(req.Text)));
                    new FullAstWalker(nodeMap, bufferMap).Visit(compileResult.CompileUnit);
                    errorsMessages.CreateErrorMessages(compileResult.Errors);
                    nodeMap.Complete(compileResult);
                    service.Invoke(new Action(service.SynchronizeDropdowns), new object[] {});
                }
// ReSharper disable EmptyGeneralCatchClause
                catch
// ReSharper restore EmptyGeneralCatchClause
                {}
            }
            if (Recompiled != null)
                Recompiled(this, EventArgs.Empty);
        }

        private void PipelineAfterStep(object sender, CompilerStepEventArgs args)
        {
            if (args.Step == ((CompilerPipeline)sender)[0])
                new ParsedAstWalker(nodeMap, bufferMap).Visit(args.Context.CompileUnit);
        }

        public event EventHandler Recompiled;

        public IList<ClassificationSpan> ClassificationSpans { get { return nodeMap.ClassificationSpans; } }

        public IEnumerable<MappedNode> GetNodes(LexicalInfo loc, Func<MappedNode, bool> filter)
        {
            return nodeMap.GetNodes(loc, filter);
        }

        public IEnumerable<MappedNode> GetNodes(int line, int pos, Func<MappedNode, bool> filter)
        {
            return nodeMap.GetNodes(line, pos, filter);
        }

        public IEnumerable<MappedNode> GetAdjacentNodes(int line, int pos, Func<MappedNode, bool> filter)
        {
            return nodeMap.GetAdjacentNodes(line, pos, filter);
        }

        internal SnapshotSpan GetSnapshotSpan(LexicalInfo lexicalInfo)
        {
            return nodeMap.GetSnapshotSpan(lexicalInfo);
        }

        internal IEnumerable<MappedNode> GetTypes()
        {
            return nodeMap.GetTypes();
        }

        public CompilerErrorCollection Errors { get { return nodeMap.Errors; } }
        public CompilerWarningCollection Warnings { get { return nodeMap.Warnings; } }

        internal int MapPosition(int line, int pos)
        {
            return bufferMap.MapPosition(line, pos);
        }

        internal SnapshotSpan SnapshotSpan { get { return new SnapshotSpan(bufferMap.CurrentSnapshot, 0, bufferMap.CurrentSnapshot.Length); } }

        public override void Dispose()
        {
            if (errorsMessages != null)
                errorsMessages.Dispose();
            base.Dispose();
        }
    }
}
