using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.BooProject.LanguageService.Colorizer
{
    class Classifier : IClassifier
    {
        private Service service;
        private IVsTextLines buffer;
        private BooSource source;
        public Classifier(Service service, IVsTextLines buffer)
        {
            this.service = service;
            this.buffer = buffer;
        }

        public Classifier(Service service, IClassificationTypeRegistryService classificationTypeRegistry, IVsTextLines iVsTextLines)
        {
            // TODO: Complete member initialization
            this.service = service;
            this.classificationTypeRegistry = classificationTypeRegistry;
            this.iVsTextLines = iVsTextLines;
        }

        #region IClassifier Members

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
        private IClassificationTypeRegistryService classificationTypeRegistry;
        private IVsTextLines iVsTextLines;

        public IList<ClassificationSpan> GetClassificationSpans(Microsoft.VisualStudio.Text.SnapshotSpan span)
        {
            if (source == null)
                source = (BooSource)service.GetSource(buffer);
            var result = new List<ClassificationSpan>();
            result.Add(new ClassificationSpan(span, classificationTypeRegistry.GetClassificationType(Formats.BooKeyword)));
            return result;
        }

        #endregion
    }
}
