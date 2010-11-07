using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Tagging;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.BooProject.LanguageService.Colorizer
{
    [Export(typeof(ITaggerProvider))]
    [ContentType("Visual Boo")]
    [TagType(typeof(ErrorTag))]
    public class TaggerProvider : ITaggerProvider
    {
        [Import]
        private IVsEditorAdaptersFactoryService bufferAdapterService;

        [Import(typeof(SVsServiceProvider))]
        private IServiceProvider serviceProvider;

        public ITagger<T> CreateTagger<T>(Microsoft.VisualStudio.Text.ITextBuffer buffer) where T : ITag
        {
            return (ITagger<T>)new Tagger((Service)serviceProvider.GetService(typeof(Service)),
                (IVsTextLines)bufferAdapterService.GetBufferAdapter(buffer));
        }

    }
}
