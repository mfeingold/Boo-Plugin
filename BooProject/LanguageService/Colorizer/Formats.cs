using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Classification;
using System.Windows.Media;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.BooProject.LanguageService.Colorizer
{
    class Formats
    {

        internal const string BooKeyword = "boo.keyword";
        [Export]
        [Name(BooKeyword)]
        private static ClassificationTypeDefinition booKeyword;

        [Export(typeof(EditorFormatDefinition))]
        [Name("boo.keyword.format")]
        [DisplayName("Boo Keyword Format")]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = BooKeyword)]
        [Order]
        internal sealed class BooKeywordFormat : ClassificationFormatDefinition
        {
            public BooKeywordFormat()
            {
                ForegroundColor = Colors.Blue;
//                BackgroundColor = Colors.Yellow;
            }
        }

        internal const string BooBlockComment = "boo.blockcomment";
        [Export]
        [Name(BooBlockComment)]
        private static ClassificationTypeDefinition booBlockComment;

        [Export(typeof(EditorFormatDefinition))]
        [Name("boo.blockcomment.format")]
        [DisplayName("Boo Blockcomment Format")]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = BooBlockComment)]
        [Order]
        internal sealed class BooBlockCommentFormat : ClassificationFormatDefinition
        {
            public BooBlockCommentFormat()
            {
                ForegroundColor = Colors.Green;
//                BackgroundColor = Colors.Yellow;
            }
        }


        // even though the "custom" colors defined here are identical to stack colors, it is necessary 
        // to configure the package to provide them as "custom" because stock colors trump colors provided
        // by classifier
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
                                                                             FONTFLAGS.FF_DEFAULT)//,
                                                           //new ColorableItem("Boo – Type",
                                                           //                  "Type",
                                                           //                  COLORINDEX.CI_AQUAMARINE,
                                                           //                  COLORINDEX.CI_SYSPLAINTEXT_BK,
                                                           //                  System.Drawing.Color.SteelBlue,
                                                           //                  System.Drawing.Color.White,
                                                           //                  FONTFLAGS.FF_DEFAULT)
                                                       };
    }
}
