using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Classification;
using System.Windows.Media;

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
                BackgroundColor = Colors.Yellow;
            }
        }
    }
}
