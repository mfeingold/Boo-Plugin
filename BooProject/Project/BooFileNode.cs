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

        void ShowMessages(IVsTextLines buffer);
    }
    
    
    public class BooFileNode : FileNode, IFileNode
    {
        private CompileResults results;
        private bool hidden;
        private bool needCompilation;

        private CompileResults GetResults()
        {
            return results;
        }

        public BooFileNode(ProjectNode root, ProjectElement e)
			: base(root, e)
		{
            results = new CompileResults(this);
            hidden = true;
            needCompilation = true;
        }

        public bool NeedsCompilation { get { return needCompilation; } }

        public ICompilerInput GetCompilerInput(CompileResults result)
        {
            IVsHierarchy hier;
            uint itemId;
            IntPtr docData;
            uint cookie;

            string source;
            if (ErrorHandler.Succeeded(
                GlobalServices.RDT.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_NoLock, Url, out hier, out itemId, out docData,
                                                   out cookie))
                && docData != IntPtr.Zero
                )
            {
                var doc = Marshal.GetObjectForIUnknown(docData);
                Marshal.Release(docData);

                var lines = doc as IVsTextLines;
                if (lines == null)
                {
                    var bufferProvider = doc as IVsTextBufferProvider;
                    if (bufferProvider != null)
                    {
                        ErrorHandler.ThrowOnFailure(bufferProvider.GetTextBuffer(out lines));
                        System.Diagnostics.Debug.Assert(lines != null, "IVsTextLines does not implement IVsTextBuffer");
                    }
                }

                int lineCount;
                ErrorHandler.ThrowOnFailure(lines.GetLineCount(out lineCount));
                int lineLength;
                ErrorHandler.ThrowOnFailure(lines.GetLengthOfLine(lineCount - 1, out lineLength));

                ErrorHandler.ThrowOnFailure(lines.GetLineText(0, 0, lineCount-1, lineLength, out source));

                hidden = false;
            }
            else
                source = File.ReadAllText(Url);

            var path = Url;
            result.Initialize(path, source);
            return new StringInput(path, source);
        }

        public void SetCompilerResults(CompileResults newResults)
        {
            results.HideMessages();
            results = newResults;
            needCompilation = false;
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

        #region IFileNode Members

        public event EventHandler Recompiled;

        public MappedToken GetMappedToken(int line, int col) { return GetResults().GetMappedToken(line, col); }

        public MappedToken GetAdjacentMappedToken(int line, int col) { return GetResults().GetAdjacentMappedToken(line, col); }

        public IEnumerable<MappedTypeDefinition> Types { get { return GetResults().Types; } }

        public CompileResults.BufferPoint MapPosition(int line, int column) { return GetResults().LocationToPoint(line, column); }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span) { return GetResults().GetClassificationSpans(span); }

        public IEnumerable<ITagSpan<ErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans) { return GetResults().GetTags(spans); }

        public void HideMessages() { GetResults().HideMessages(); }

        public void ShowMessages(IVsTextLines buffer)
        {
            GetResults().ShowMessages();
        }

        #endregion
    }
}
