using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boo.Lang.Compiler.Ast;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text;
using Boo.Lang.Parser;
using Hill30.BooProject.LanguageService.Colorizer;

namespace Hill30.BooProject.LanguageService.NodeMapping
{
    public class Mapper
    {
        private readonly int[][] positionMap;
        private readonly Dictionary<int, List<MappedNode>> nodeDictionary = new Dictionary<int, List<MappedNode>>();
        private readonly ITextSnapshot currentSnapshot;
        private readonly IClassificationTypeRegistryService iClassificationTypeRegistryService;
        private int currentPos = 0;
        private readonly List<ClassificationSpan> classificationSpans = new List<ClassificationSpan>();
        public IList<ClassificationSpan> ClassificationSpans { get { return classificationSpans; } }

        public Mapper(IClassificationTypeRegistryService iClassificationTypeRegistryService, ITextBuffer buffer, int tabSize)
        {
            this.iClassificationTypeRegistryService = iClassificationTypeRegistryService;
            currentSnapshot = buffer.CurrentSnapshot;
            var source = currentSnapshot.GetText();

            var sourcePos = 0;
            var mappedPos = 0;
            var mappers = new List<int[]>();
            var positionList = new List<int>();
            foreach (var c in source)
            {
                if (c == '\t')
                    while (mappedPos % tabSize < tabSize - 1)
                    {
                        positionList.Add(sourcePos);
                        mappedPos++;
                    }
                positionList.Add(sourcePos++);
                mappedPos++;
                if (c=='\n')
                {
                    mappers.Add(positionList.ToArray());
                    positionList.Clear();
                    sourcePos = 0;
                    mappedPos = 0;
                }
            }
            positionList.Add(sourcePos); // to map the <EOL> token
            mappers.Add(positionList.ToArray());
            positionMap = mappers.ToArray();
        }

        internal void MapToken(antlr.IToken token)
        {
            if (token.Type == BooLexer.EOL)
                return;

            if (token.Type == BooLexer.INDENT)
                return;

            if (token.Type == BooLexer.DEDENT)
                return;

            var start = positionMap[token.getLine() - 1][token.getColumn() - 1];

            var length = token.getText().Length;
            if (token.Type == BooLexer.TRIPLE_QUOTED_STRING)
                length += 6;
            if (token.Type == BooLexer.DOUBLE_QUOTED_STRING)
                length += 4;
            if (token.Type == BooLexer.SINGLE_QUOTED_STRING)
                length += 4;

            var end = positionMap[token.getLine() - 1][token.getColumn() - 1 + length];
            length = end - start;

            var span =
                new SnapshotSpan(currentSnapshot,
                    currentSnapshot.GetLineFromLineNumber(token.getLine() - 1).Start + start,
                    length);
            if (span.Start > currentPos)
                classificationSpans.Add(new ClassificationSpan
                    (new SnapshotSpan(currentSnapshot, currentPos, span.Start - currentPos),
                    iClassificationTypeRegistryService.GetClassificationType(Formats.BooBlockComment)
                    ));

            var format = tokenFormats[(int)GetTokenType(token)];
            if (format != null)
            {
                classificationSpans.Add(new ClassificationSpan(span, iClassificationTypeRegistryService.GetClassificationType(format)));
            }

            currentPos = span.End;

        }

        public void MapNode(MappedNode node)
        {
            List<MappedNode> nodes;
            if (!nodeDictionary.TryGetValue(node.Line-1, out nodes))
                nodeDictionary[node.Line-1] = nodes = new List<MappedNode>();
            nodes.Add(node);
        }

        private MappedNode GetNode(int line, int pos, Func<MappedNode, int, int, bool> comparer)
        {
            List<MappedNode> nodes;
            if (!nodeDictionary.TryGetValue(line, out nodes))
                return null;
            MappedNode result = null;
            foreach (var node in nodes)
            {
                if (comparer(node, line, pos))
                    result = node;
            }
            return result;
        }

        public MappedNode GetNode(int line, int pos)
        {
            return GetNode(line, pos, (node, li, po) => (po >= node.StartPos && po < node.EndPos));
        }

        internal MappedNode GetAdjacentNode(int line, int pos)
        {
            return GetNode(line, pos, (node, li, po) => (po > node.EndPos));
        }

        internal Tuple<int, int> MapLocation(int lineNo, int pos, int length)
        {
            return new Tuple<int, int>(
                positionMap[lineNo - 1][pos - 1],
                positionMap[lineNo - 1][pos - 1 + length]
                );
        }

        internal void Complete()
        {
            foreach (var list in nodeDictionary.Values)
                foreach (var node in list)
                {
                    node.Resolve();
                    if (node.Format != null)
                    {
                        var start = currentSnapshot.GetLineFromLineNumber(node.Line - 1).Start + node.StartPos;
                        var span = new SnapshotSpan(currentSnapshot, start, node.EndPos - node.StartPos);
                        classificationSpans.Add(new ClassificationSpan(span,
                                                                       iClassificationTypeRegistryService.GetClassificationType(
                                                                           node.Format)));
                    }
                }

            if (currentPos < currentSnapshot.Length-1)
                classificationSpans.Add(
                    new ClassificationSpan(
                        new SnapshotSpan(currentSnapshot, currentPos, currentSnapshot.Length - currentPos),
                        iClassificationTypeRegistryService.GetClassificationType(Formats.BooBlockComment)
                        ));
        }

        internal SnapshotSpan SnapshotSpan { get { return new SnapshotSpan(currentSnapshot, 0, currentSnapshot.Length); } }

        static readonly string[] tokenFormats = 
        {
            null,
            null,
            null,
            null,
            null,
            Formats.BooKeyword,
            null
        };

        public enum TokenType
        {
            DocumentString = 0,
            String = 1,
            MemberSelector = 2,
            WhiteSpace = 3,
            Identifier = 4,
            Keyword = 5,
            Other = 6,
        }

        public static TokenType GetTokenType(antlr.IToken token)
        {
            switch (token.Type)
            {
                case BooLexer.TRIPLE_QUOTED_STRING: return TokenType.DocumentString;

                case BooLexer.DOUBLE_QUOTED_STRING:
                case BooLexer.SINGLE_QUOTED_STRING:
                    return TokenType.String;

                case BooLexer.DOT: return TokenType.MemberSelector;

                case BooLexer.WS: return TokenType.WhiteSpace;

                case BooLexer.ID: return TokenType.Identifier;

                case BooLexer.ABSTRACT:
                case BooLexer.AS:
                case BooLexer.BREAK:
                case BooLexer.CLASS:
                case BooLexer.CONSTRUCTOR:
                case BooLexer.CONTINUE:
                case BooLexer.DEF:
                case BooLexer.DO:
                case BooLexer.ELIF:
                case BooLexer.ELSE:
                case BooLexer.ENUM:
                case BooLexer.EVENT:
                case BooLexer.EXCEPT:
                case BooLexer.FALSE:
                case BooLexer.FINAL:
                case BooLexer.FOR:
                case BooLexer.FROM:
                case BooLexer.GET:
                case BooLexer.GOTO:
                case BooLexer.IF:
                case BooLexer.IMPORT:
                case BooLexer.IN:
                case BooLexer.INTERFACE:
                case BooLexer.INTERNAL:
                case BooLexer.IS:
                case BooLexer.LONG:
                case BooLexer.NAMESPACE:
                case BooLexer.NULL:
                case BooLexer.OF:
                case BooLexer.OVERRIDE:
                case BooLexer.PARTIAL:
                case BooLexer.PASS:
                case BooLexer.PRIVATE:
                case BooLexer.PROTECTED:
                case BooLexer.PUBLIC:
                case BooLexer.RAISE:
                case BooLexer.REF:
                case BooLexer.RETURN:
                case BooLexer.SELF:
                case BooLexer.SET:
                case BooLexer.STATIC:
                case BooLexer.STRUCT:
                case BooLexer.SUPER:
                case BooLexer.THEN:
                case BooLexer.TRUE:
                case BooLexer.TRY:
                case BooLexer.TYPEOF:
                case BooLexer.UNLESS:
                case BooLexer.VIRTUAL:
                case BooLexer.WHILE:
                case BooLexer.YIELD:
                    return TokenType.Keyword;

                default: return TokenType.Other;
            }
        }
    }
}
