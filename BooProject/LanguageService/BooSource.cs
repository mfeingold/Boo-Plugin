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
using System.IO;
using System.Runtime.InteropServices;
using Boo.Lang.Compiler;
using Boo.Lang.Parser;
using Hill30.BooProject.LanguageService.Mapping;
using Hill30.BooProject.LanguageService.Mapping.Nodes;
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
using Boo.Lang.Compiler.IO;

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
                if (errorsMessages != null)
                    errorsMessages.Dispose();
                errorsMessages = new Collection(projectManager, hierarchyItem);
                var lexer = BooParser.CreateBooLexer(service.GetLanguagePreferences().TabSize, "code stream", new StringReader(req.Text));
                try
                {
                    nodeMap.Clear();
                    nodeMap.MapTokens(lexer);
                    if (compiler == null)
                    {
                        compiler = projectManager.CreateCompiler();
                        compiler.Parameters.Pipeline.AfterStep += PipelineAfterStep;
                    }
                    var parsingStep = (BooParsingStep)compiler.Parameters.Pipeline.Get(typeof(BooParsingStep));
                    parsingStep.TabSize = service.GetLanguagePreferences().TabSize;
                    compiler.Parameters.Input.Clear();
                    compiler.Parameters.Input.Add(new StringInput(bufferMap.FilePath, req.Text));
                    compileResult = compiler.Run();
                    
                    new FullAstWalker(nodeMap, bufferMap).Visit(compileResult.CompileUnit);
                    errorsMessages.CreateMessages(compileResult.Errors, compileResult.Warnings);
                    nodeMap.Complete(compileResult);
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

        internal IEnumerable<MappedTypeDefinition> GetTypes()
        {
            return nodeMap.GetTypes();
        }

        public CompilerErrorCollection Errors { get { return nodeMap.Errors; } }
        public CompilerWarningCollection Warnings { get { return nodeMap.Warnings; } }

        internal BufferMap.BufferPoint MapPosition(int line, int pos)
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
