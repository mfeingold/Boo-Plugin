//
//   Copyright © 2010 Michael Feingold
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using Boo.Lang.Compiler.Ast;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.Boo.ASTMapper
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

        public static TextSpan GetTextSpan(this Node node, IFileNode fileNode)
        {
            var start = fileNode.MapPosition(node.LexicalInfo.Line, node.LexicalInfo.Column);
            var end = fileNode.MapPosition(node.EndSourceLocation.Line, node.EndSourceLocation.Column);
            return new TextSpan
            {
                iStartLine = start.Line,
                iStartIndex = start.Column,
                iEndLine = end.Line,
                iEndIndex = end.Column
            };
        }

    }
}
