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
using System.Runtime.InteropServices;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.TypeSystem;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Boo.Lang.Compiler.IO;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;
using Microsoft.VisualStudio.TextManager.Interop;
using Hill30.BooProject.AST;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text.Classification;
using Hill30.BooProject.AST.Nodes;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text;

namespace Hill30.BooProject.Project
{
    [ComVisible(true)]
    public interface IFileNode
    {
        event EventHandler Recompiled;
        
        MappedToken GetMappedToken(int line, int col);

        MappedToken GetAdjacentMappedToken(int line, int col);

        IEnumerable<MappedTypeDefinition> Types { get; }

        CompileResults.BufferPoint MapPosition(int line, int column);

        IEnumerable<ITagSpan<ErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans);

        IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span);

        void HideMessages();

        void ShowMessages();

        void Bind(ITextBuffer textBuffer);

        void SubmitForCompile();
    }
    
    
    public class BooFileNode : FileNode, IFileNode
    {
        private CompileResults results;
        private ITextBuffer textBuffer;
        private ITextSnapshot originalSnapshot;
        private bool hidden;

        private CompileResults GetResults()
        {
            return results;
        }

        public BooFileNode(ProjectNode root, ProjectElement e)
			: base(root, e)
		{
            results = new CompileResults(this);//, textBuffer);
            hidden = true;
        }

        public void Bind(ITextBuffer textBuffer)
        {
            this.textBuffer = textBuffer;
            if (textBuffer == null)
                hidden = true;
            else
                originalSnapshot = textBuffer.CurrentSnapshot;
        }

        public ICompilerInput GetCompilerInput(CompileResults result)
        {
            if (textBuffer == null)
                return result.Initialize(File.ReadAllText(Url));
            hidden = false;
            originalSnapshot = textBuffer.CurrentSnapshot;
            return result.Initialize(originalSnapshot.GetText());
        }

        public void SetCompilerResults(CompileResults newResults)
        {
            results.HideMessages();
            results = newResults;
            if (!hidden)
                results.ShowMessages();
            if (Recompiled != null)
                Recompiled(this, EventArgs.Empty);
        }

        /// <summary>
        /// Return an imageindex
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        public override int ImageIndex
        {
            get
            {
                if (Path.GetExtension(FileName) == ".boo")
                    return BooProjectNode.ImageOffset + (int)BooProjectNode.ProjectIcons.File;
                return base.ImageIndex;
            }
        }

        #region Private implementation
   
        internal OleServiceProvider.ServiceCreatorCallback ServiceCreator
        {
            get { return CreateServices; }
        }

        private object CreateServices(Type serviceType)
        {
            object service = null;
            if (typeof(EnvDTE.ProjectItem) == serviceType)
            {
                service = GetAutomationObject();
            }
            return service;
        }
        
        #endregion

        public ICompileUnit CompileUnit { get { return GetResults().CompileUnit; } }

        private SnapshotSpan SnapshotCreator(TextSpan textspan)
        {
            if (textBuffer == null)
                return default(SnapshotSpan);

            var startIndex = originalSnapshot.GetLineFromLineNumber(textspan.iStartLine).Start + textspan.iStartIndex;
            var endLine = originalSnapshot.GetLineFromLineNumber(textspan.iEndLine);
            if (textspan.iEndIndex == -1)
                return new SnapshotSpan(originalSnapshot, startIndex, endLine.Start + endLine.Length - startIndex);
            else
                return new SnapshotSpan(originalSnapshot, startIndex, endLine.Start + textspan.iEndIndex - startIndex);
        }
        
        #region IFileNode Members

        public event EventHandler Recompiled;

        public MappedToken GetMappedToken(int line, int col) { return GetResults().GetMappedToken(line, col); }

        public MappedToken GetAdjacentMappedToken(int line, int col) { return GetResults().GetAdjacentMappedToken(line, col); }

        public IEnumerable<MappedTypeDefinition> Types { get { return GetResults().Types; } }

        public CompileResults.BufferPoint MapPosition(int line, int column) { return GetResults().LocationToPoint(line, column); }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span) { return GetResults().GetClassificationSpans(span, SnapshotCreator); }

        public IEnumerable<ITagSpan<ErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans) { return GetResults().GetTags(spans, SnapshotCreator); }

        public void HideMessages() { GetResults().HideMessages(); }

        public void ShowMessages() { GetResults().ShowMessages(); }

        public void SubmitForCompile() { ((BooProjectNode) ProjectMgr).SubmitForCompile(this); }

        #endregion

    }
}
