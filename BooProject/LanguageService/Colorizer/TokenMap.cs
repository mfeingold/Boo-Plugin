using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.BooProject.LanguageService.Colorizer
{
    public class TokenMap : List<ClassificationSpan>
    {
        private readonly ITextSnapshot currentSnapshot;
        private readonly IClassificationTypeRegistryService iClassificationTypeRegistryService;

        public TokenMap(IClassificationTypeRegistryService iClassificationTypeRegistryService, ITextBuffer buffer)
        {
            this.iClassificationTypeRegistryService = iClassificationTypeRegistryService;
            currentSnapshot = buffer.CurrentSnapshot;
        }

        internal void MapToken(antlr.IToken token)
        {

            var span = 
                new SnapshotSpan(currentSnapshot, 
                    currentSnapshot.GetLineFromLineNumber(token.getLine() - 1).Extent);

            Add(new ClassificationSpan(span, iClassificationTypeRegistryService.GetClassificationType(Formats.BooKeyword)));

        }

        internal void CompleteComments()
        {
            var body = new SnapshotSpan(currentSnapshot, 0, currentSnapshot.Length).Span;
            foreach (var item in new List<ClassificationSpan>(this))
            {
                if (body.Start < item.Span.Start)
                    Add(
                        new ClassificationSpan(
                            new SnapshotSpan(currentSnapshot, body.Start, item.Span.Start - body.Start),
                            iClassificationTypeRegistryService.GetClassificationType(Formats.BooBlockComment)
                            ));
                body = new Span(item.Span.End, currentSnapshot.Length - item.Span.End);
            }
        }

        internal SnapshotSpan SnapshotSpan { get { return new SnapshotSpan(currentSnapshot, 0, currentSnapshot.Length); } }
    }
}
