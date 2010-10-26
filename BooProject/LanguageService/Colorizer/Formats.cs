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
        //[Export]
        //[Name("Boo")]
        //[BaseDefinition("code")]
        //[BaseDefinition("projection")]
        //internal static ContentTypeDefinition BooContentTypeDefinition;

        //[Export]
        //[FileExtension(".boo")]
        //[ContentType("Boo")]
        //internal static ContentTypeDefinition BooFileExtensionDefinition;

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
                BackgroundColor = Colors.Yellow;
            }
        }
    }
}
