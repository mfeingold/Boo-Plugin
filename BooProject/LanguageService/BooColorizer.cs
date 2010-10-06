using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.BooProject.LanguageService
{
    public class BooColorizer : Colorizer
    {
        BooSource source;

        public BooColorizer(Service service, IVsTextLines buffer, IScanner scanner)
            : base(service, buffer, scanner)
        {
            source = (BooSource)service.GetSource(buffer);
        }

        public override int ColorizeLine(int line, int length, IntPtr ptr, int state, uint[] attrs)
        {
            ((Scanner)Scanner).SetLineNumber(line);
            var result = base.ColorizeLine(line, length, ptr, state, attrs);
            ((Scanner)Scanner).SetLineNumber(-1);
            return result;
        }

        public override TokenInfo[] GetLineInfo(IVsTextLines buffer, int line, IVsTextColorState colorState)
        {
            ((Scanner)Scanner).SetLineNumber(line);
            var result = base.GetLineInfo(buffer, line, colorState);
            ((Scanner)Scanner).SetLineNumber(-1);
            return result;
        }

    }
}
