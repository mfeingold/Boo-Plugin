using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.BooProject.LanguageService.Colorizer
{
    class Classifier : IClassifier
    {
        private readonly Service service;
        private readonly IVsTextLines buffer;
        private BooSource source;

        public Classifier(Service service, IVsTextLines buffer)
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
                    source.Recompiled += source_Recompiled;
            }

            if (source != null && source.Mapper != null)
                return source.Mapper.ClassificationSpans;
            
            return new List<ClassificationSpan>();
        }

        #endregion

        void source_Recompiled(object sender, EventArgs e)
        {
            if (ClassificationChanged != null)
                ClassificationChanged(sender, new ClassificationChangedEventArgs(source.Mapper.SnapshotSpan));
        }
    }
}
