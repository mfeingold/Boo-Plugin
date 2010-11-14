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
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.BooProject.LanguageService.Colorizer
{
    class Classifier : IClassifier
    {
        private readonly BooLanguageService service;
        private readonly IVsTextLines buffer;
        private BooSource source;

        public Classifier(BooLanguageService service, IVsTextLines buffer)
        {
            this.service = service;
            this.buffer = buffer;
        }

        #region IClassifier Members

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            if (source == null)
            {
                source = (BooSource) service.GetSource(buffer);
                if (source != null)
                    source.Recompiled += SourceRecompiled;
            }

            if (source != null)
                return source.ClassificationSpans;
            
            return new List<ClassificationSpan>();
        }

        #endregion

        void SourceRecompiled(object sender, EventArgs e)
        {
            if (ClassificationChanged != null)
                ClassificationChanged(sender, new ClassificationChangedEventArgs(source.SnapshotSpan));
        }
    }
}
