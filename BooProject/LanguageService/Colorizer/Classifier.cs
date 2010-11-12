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
