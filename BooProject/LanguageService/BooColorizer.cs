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

        public enum TokenColor
        {
            Text,
            Keyword,
            Comment,
            Identifier,
            String,
            Number,
            Type
        }

        public static readonly ColorableItem[] ColorableItems = new[]
                                                       {
                                                           new ColorableItem("Boo – Text",
                                                                             "Text",
                                                                             COLORINDEX.CI_SYSPLAINTEXT_FG,
                                                                             COLORINDEX.CI_SYSPLAINTEXT_BK,
                                                                             System.Drawing.Color.Empty,
                                                                             System.Drawing.Color.Empty,
                                                                             FONTFLAGS.FF_DEFAULT),
                                                           new ColorableItem("Boo – Keyword",
                                                                             "Keyword",
                                                                             COLORINDEX.CI_BLUE,
                                                                             COLORINDEX.CI_SYSPLAINTEXT_BK,
                                                                             System.Drawing.Color.Blue,
                                                                             System.Drawing.Color.White,
                                                                             FONTFLAGS.FF_DEFAULT),
                                                           new ColorableItem("Boo – Comment",
                                                                             "Comment",
                                                                             COLORINDEX.CI_GREEN,
                                                                             COLORINDEX.CI_SYSPLAINTEXT_BK,
                                                                             System.Drawing.Color.Green,
                                                                             System.Drawing.Color.White,
                                                                             FONTFLAGS.FF_DEFAULT),
                                                           new ColorableItem("Boo – Identifier",
                                                                             "Identifier",
                                                                             COLORINDEX.CI_SYSPLAINTEXT_FG,
                                                                             COLORINDEX.CI_SYSPLAINTEXT_BK,
                                                                             System.Drawing.Color.Black,
                                                                             System.Drawing.Color.White,
                                                                             FONTFLAGS.FF_DEFAULT),
                                                           new ColorableItem("Boo – String",
                                                                             "String",
                                                                             COLORINDEX.CI_MAROON,
                                                                             COLORINDEX.CI_SYSPLAINTEXT_BK,
                                                                             System.Drawing.Color.Maroon,
                                                                             System.Drawing.Color.White,
                                                                             FONTFLAGS.FF_DEFAULT),
                                                           new ColorableItem("Boo – Number",
                                                                             "Number",
                                                                             COLORINDEX.CI_SYSPLAINTEXT_FG,
                                                                             COLORINDEX.CI_SYSPLAINTEXT_BK,
                                                                             System.Drawing.Color.Black,
                                                                             System.Drawing.Color.White,
                                                                             FONTFLAGS.FF_DEFAULT),
                                                           new ColorableItem("Boo – Type",
                                                                             "Type",
                                                                             COLORINDEX.CI_AQUAMARINE,
                                                                             COLORINDEX.CI_SYSPLAINTEXT_BK,
                                                                             System.Drawing.Color.SteelBlue,
                                                                             System.Drawing.Color.White,
                                                                             FONTFLAGS.FF_DEFAULT)
                                                       };
    }
}
