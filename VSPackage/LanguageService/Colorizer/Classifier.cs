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
using Microsoft.VisualStudio.Text.Classification;

namespace Hill30.BooProject.LanguageService.Colorizer
{
    class Classifier : IClassifier
    {
        private ITextBuffer buffer;
        private IFileNode fileNode;

        public Classifier(ITextBuffer buffer)
        {
            this.buffer = buffer;
            fileNode = GlobalServices.GetFileNodeForBuffer(buffer);
            if (fileNode != null)
                fileNode.Recompiled += SourceRecompiled;
        }

        #region IClassifier Members

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {

            if (fileNode != null)
                return fileNode.GetClassificationSpans(span);
            
            return new List<ClassificationSpan>();
        }

        #endregion

        void SourceRecompiled(object sender, EventArgs e)
        {
            if (ClassificationChanged != null)
                ClassificationChanged(sender, new ClassificationChangedEventArgs(new SnapshotSpan(buffer.CurrentSnapshot, 0, buffer.CurrentSnapshot.Length)));
        }
    }
}
