using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.BooProject.LanguageService
{
    public class BooParseRequest : ParseRequest
    {
        public BooParseRequest(BooSource source, int line, int idx, TokenInfo info, string sourceText, string fname, ParseReason reason, IVsTextView view, AuthoringSink sink,bool synchronous)
            : base(line, idx, info, sourceText, fname, reason, view, sink, synchronous)
        { Source = source; }

        public BooSource Source { get; private set; }
    }
}
