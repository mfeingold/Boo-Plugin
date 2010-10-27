using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;

namespace Hill30.BooProject.LanguageService.Colorizer
{
    [Export(typeof(IClassifierProvider))]
    [ContentType("Visual Boo")]
    [Name("Boo Classifier")]
    internal class ClassifierProvider : IClassifierProvider
    {
        [Import] 
        private IVsEditorAdaptersFactoryService bufferAdapterService;

        [Import(typeof(SVsServiceProvider))]
        private IServiceProvider serviceProvider;

        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            return new Classifier(
                (Service)serviceProvider.GetService(typeof(Service)), 
                (IVsTextLines)bufferAdapterService.GetBufferAdapter(textBuffer));
        }
    }
}
