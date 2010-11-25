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
using Microsoft.VisualStudio.Text.Classification;
using System.Collections.Generic;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.IO;

namespace Hill30.BooProject.LanguageService
{
    public class BooSource : Source
    {
        private readonly BooLanguageService service;
        private readonly NodeMap nodeMap;
        private readonly ITextBuffer buffer;
        private readonly IProjectManager projectManager;

        public BooSource(BooLanguageService service, IVsTextLines buffer, Microsoft.VisualStudio.Package.Colorizer colorizer)
            : base(service, buffer, colorizer)
        {
            this.service = service;
            this.buffer = service.BufferAdapterService.GetDataBuffer(buffer);
            
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            var hierarchyItem = new RunningDocumentTable(this.service.Site).GetHierarchyItem(GetFilePath());
            // ReSharper restore DoNotCallOverridableMethodsInConstructor

            object value;
            ErrorHandler.ThrowOnFailure(hierarchyItem.GetProperty(VSConstants.VSITEMID_ROOT, (int) __VSHPROPID.VSHPROPID_Root, out value));
            var pointer = new IntPtr((int)value);
            try
            {
                projectManager = Marshal.GetObjectForIUnknown(pointer) as IProjectManager;
                nodeMap = new NodeMap(service, projectManager, hierarchyItem, this.buffer);
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
                var lexer = BooParser.CreateBooLexer(service.GetLanguagePreferences().TabSize, "code stream", new StringReader(req.Text));
                try
                {
                    projectManager.Compile();
                    nodeMap.Initialize();
                    nodeMap.MapTokens(lexer);
                    if (compiler == null)
                    {
                        compiler = projectManager.CreateCompiler();
                        compiler.Parameters.Pipeline.AfterStep += PipelineAfterStep;
                    }
                    var parsingStep = (BooParsingStep)compiler.Parameters.Pipeline.Get(typeof(BooParsingStep));
                    parsingStep.TabSize = service.GetLanguagePreferences().TabSize;
                    compiler.Parameters.Input.Clear();
                    compiler.Parameters.Input.Add(new StringInput(GetFilePath(), req.Text));
                    var compileResult = compiler.Run();
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
                nodeMap.MapParsedNodes(args.Context);
        }

        public event EventHandler Recompiled;

        public IList<ClassificationSpan> ClassificationSpans { get { return nodeMap.ClassificationSpans; } }
        public CompilerErrorCollection Errors { get { return nodeMap.Errors; } }
        public CompilerWarningCollection Warnings { get { return nodeMap.Warnings; } }

        internal BufferMap.BufferPoint MapPosition(int line, int pos)
        {
            return nodeMap.MapPosition(line, pos);
        }

        internal SnapshotSpan SnapshotSpan { get { return nodeMap.SnapshotSpan; } }

        internal SnapshotSpan GetErrorSnapshotSpan(LexicalInfo lexicalInfo)
        {
            return nodeMap.GetErrorSnapshotSpan(lexicalInfo);
        }

        internal IEnumerable<MappedTypeDefinition> GetTypes()
        {
            return nodeMap.GetTypes();
        }

        internal string GetDataTipText(int line, int col, out TextSpan span)
        {
            var cluster = nodeMap.GetNodeCluster(line, col);
            if (cluster == null)
            {
                span = new TextSpan();
                return "";
            }
            return cluster.GetDataTiptext(out span);
        }

        internal Declarations GetDeclarations(int line, int col, TokenInfo info, ParseReason reason)
        {
            var cluster = nodeMap.GetAdjacentNodeCluster(line, col);
            if (cluster == null)
                return new BooDeclarations();
            return cluster.GetDeclarations(info, reason);
        }

        internal string Goto(int line, int col, out TextSpan span)
        {
            var cluster = nodeMap.GetNodeCluster(line, col);
            if (cluster == null)
            {
                span = new TextSpan();
                return null;
            }
            return cluster.Goto(out span);
        }

        public override CommentInfo GetCommentFormat()
        {
            return new CommentInfo {BlockStart = "/*", BlockEnd = "*/", UseLineComments = false};
        }

        public override void Dispose()
        {
            nodeMap.ClearErrorMessages();
            base.Dispose();
        }

    }
}
