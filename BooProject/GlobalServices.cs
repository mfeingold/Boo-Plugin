using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Hill30.BooProject.LanguageService;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Hill30.BooProject
{
    public class GlobalServices
    {

        public static readonly IVsRunningDocumentTable RDT = (IVsRunningDocumentTable)Package.GetGlobalService(typeof(SVsRunningDocumentTable));

        public static readonly BooLanguageService LanguageService = (BooLanguageService)Package.GetGlobalService(typeof (BooLanguageService));

    }
}
