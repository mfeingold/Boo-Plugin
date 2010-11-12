using System;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Text;

namespace Hill30.BooProject.LanguageService
{
    public static class TextSpanExtensions
    {
        public static TextSpan Union(this TextSpan self, TextSpan span)
        {
            if (self.iEndIndex == 0 && self.iEndLine == 0 && self.iStartIndex == 0 && self.iStartLine == 0)
                return span;
            return new TextSpan
                       {
                           iStartLine = Math.Min(self.iStartLine, span.iStartLine),
                           iStartIndex = 
                                self.iStartLine < span.iStartLine 
                                    ? self.iStartIndex
                                    : self.iStartLine > span.iStartLine 
                                        ? span.iStartIndex
                                        : Math.Min(self.iStartIndex, span.iStartIndex),
                           iEndLine = Math.Max(self.iEndLine, span.iEndLine),
                           iEndIndex =
                                self.iEndLine < span.iEndLine
                                    ? span.iEndIndex
                                    : self.iEndLine > span.iEndLine
                                        ? self.iEndIndex
                                        : Math.Max(self.iEndIndex, span.iEndIndex),
                       };
        }

        public static Span GetSpan(this TextSpan self, ITextSnapshot snapshot)
        {
            var start = snapshot.GetLineFromLineNumber(self.iStartLine).Start + self.iStartIndex;
            var end = snapshot.GetLineFromLineNumber(self.iEndLine).Start + self.iEndIndex;
            return new Span(start, end - start);
        }

        public static SnapshotSpan GetSnapshotSpan(this TextSpan self, ITextSnapshot snapshot)
        {
            return new SnapshotSpan(snapshot, self.GetSpan(snapshot));
        }

        public static bool Contains(this TextSpan self, int line, int column)
        {
            if (line < self.iStartLine)
                return false;
            if (line == self.iStartLine && column < self.iStartIndex)
                return false;
            if (line == self.iEndLine && column > self.iEndIndex)
                return false;
            if (line > self.iEndLine)
                return false;
            return true;
        }

    }
}
