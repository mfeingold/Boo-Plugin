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
using System.Collections.Generic;
using Hill30.Boo.ASTMapper;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace Hill30.BooProject.LanguageService.Colorizer
{
    class Tagger : ITagger<ErrorTag>
    {
        private ITextBuffer buffer;
        private IFileNode fileNode;

        public Tagger(ITextBuffer buffer)
        {
            this.buffer = buffer;
            fileNode = GlobalServices.GetFileNodeForBuffer(buffer);
            if (fileNode != null)
                fileNode.Recompiled += SourceRecompiled;
        }

        #region ITagger<ErrorTag> Members

        public IEnumerable<ITagSpan<ErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (fileNode != null)
                return fileNode.GetTags(spans);
            return new List<ITagSpan<ErrorTag>>();
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        #endregion

        void SourceRecompiled(object sender, EventArgs e)
        {
            if (TagsChanged != null)
                TagsChanged(sender, new SnapshotSpanEventArgs(new SnapshotSpan(buffer.CurrentSnapshot, 0, buffer.CurrentSnapshot.Length)));
        }
    }
}
