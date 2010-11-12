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
                foreach (var error in source.Errors)
                    yield return new TagSpan<ErrorTag>(source.GetSnapshotSpan(error.LexicalInfo)
                        , new ErrorTag(PredefinedErrorTypeNames.SyntaxError, error.Code + ' ' + error.Message));

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
