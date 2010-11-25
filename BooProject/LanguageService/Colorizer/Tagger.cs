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
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;

namespace Hill30.BooProject.LanguageService.Colorizer
{
    class Tagger : ITagger<ErrorTag>
    {
        private readonly BooLanguageService service;
        private readonly IVsTextLines buffer;
        private BooSource source;

        public Tagger(BooLanguageService service, IVsTextLines iVsTextLines)
        {
            this.service = service;
            buffer = iVsTextLines;
        }

        #region ITagger<ErrorTag> Members

        public IEnumerable<ITagSpan<ErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (source == null)
            {
                source = (BooSource)service.GetSource(buffer);
                if (source != null)
                    source.Recompiled += SourceRecompiled;
            }

            if (source != null)
            {
                foreach (var error in source.Errors)
                    yield return new TagSpan<ErrorTag>(source.GetErrorSnapshotSpan(error.LexicalInfo),
                                                       new ErrorTag(PredefinedErrorTypeNames.SyntaxError,
                                                                    "Error: " + error.Code + ' ' + error.Message));
                foreach (var error in source.Warnings)
                    yield return new TagSpan<ErrorTag>(source.GetErrorSnapshotSpan(error.LexicalInfo),
                                                       new ErrorTag(PredefinedErrorTypeNames.Warning,
                                                                    "Warning: " + error.Code + ' ' + error.Message));
            }
            yield break;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        #endregion
        void SourceRecompiled(object sender, EventArgs e)
        {
            if (TagsChanged != null)
                TagsChanged(sender, new SnapshotSpanEventArgs(source.SnapshotSpan));
        }
    }
}
