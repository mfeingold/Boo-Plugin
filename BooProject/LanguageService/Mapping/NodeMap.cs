using System;
using System.Collections.Generic;
using System.Linq;
using Boo.Lang.Parser;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text;
using Hill30.BooProject.LanguageService.Colorizer;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler;

namespace Hill30.BooProject.LanguageService.Mapping
{
    public class NodeMap
    {
        private readonly Dictionary<int, List<MappedNode>> nodeDictionary = new Dictionary<int, List<MappedNode>>();
        private readonly List<ClassificationSpan> classificationSpans = new List<ClassificationSpan>();
        BooLanguageService service;
        BufferMap bufferMap;

        public CompilerErrorCollection Errors { get; private set; }
        public CompilerWarningCollection Warnings { get; private set; }
        public IList<ClassificationSpan> ClassificationSpans { get { return classificationSpans; } }

        public NodeMap(BooLanguageService service, BufferMap bufferMap)
        {
            this.service = service;
            this.bufferMap = bufferMap;
            Errors = new CompilerErrorCollection();
            Warnings = new CompilerWarningCollection();
        }

        internal void Clear()
        {
            nodeDictionary.Clear();
            classificationSpans.Clear();
            Errors.Clear();
            Warnings.Clear();
        }

        internal void MapTokens(antlr.TokenStream tokens)
        {
            antlr.IToken token;
            var currentPos = 0;

            while ((token = tokens.nextToken()).Type != BooLexer.EOF)
            {

                if (token.Type == BooLexer.EOL)
                    return;

                if (token.Type == BooLexer.INDENT)
                    return;

                if (token.Type == BooLexer.DEDENT)
                    return;

                var start = bufferMap.MapPosition(token.getLine(), token.getColumn());

                var length = token.getText().Length;
                if (token.Type == BooLexer.TRIPLE_QUOTED_STRING)
                    length += 6;
                if (token.Type == BooLexer.DOUBLE_QUOTED_STRING)
                    length += 2;
                if (token.Type == BooLexer.SINGLE_QUOTED_STRING)
                    length += 2;

                var end = bufferMap.MapPosition(token.getLine(), token.getColumn() + length);
                length = end - start;

                var span =
                    new SnapshotSpan(bufferMap.CurrentSnapshot,
                        bufferMap.CurrentSnapshot.GetLineFromLineNumber(token.getLine() - 1).Start + start,
                        length);

                if (span.Start > currentPos)
                    classificationSpans.Add(new ClassificationSpan
                        (new SnapshotSpan(bufferMap.CurrentSnapshot, currentPos, span.Start - currentPos),
                        service.ClassificationTypeRegistry.GetClassificationType(Formats.BooBlockComment)
                        ));

                var format = tokenFormats[(int)GetTokenType(token)];
                if (format != null)
                {
                    classificationSpans.Add(new ClassificationSpan(span, service.ClassificationTypeRegistry.GetClassificationType(format)));
                }

                currentPos = span.End;
            }

            if (currentPos < bufferMap.CurrentSnapshot.Length - 1)
                classificationSpans.Add(
                    new ClassificationSpan(
                        new SnapshotSpan(bufferMap.CurrentSnapshot, currentPos, bufferMap.CurrentSnapshot.Length - currentPos),
                        service.ClassificationTypeRegistry.GetClassificationType(Formats.BooBlockComment)
                        ));
        }

        public void MapNode(MappedNode node)
        {
            List<MappedNode> nodes;
            if (!nodeDictionary.TryGetValue(node.Line - 1, out nodes))
                nodeDictionary[node.Line - 1] = nodes = new List<MappedNode>();
            nodes.Add(node);
        }

        internal void MapType(ClassDefinition node)
        {
//            throw new NotImplementedException();
        }

        internal void MapType(Module node)
        {
//            throw new NotImplementedException();
        }

        internal void Complete(CompilerContext compileResult)
        {
            foreach (var list in nodeDictionary.Values)
                foreach (var node in list)
                {
                    node.Resolve();
                    if (node.Format != null)
                    {
                        var start = bufferMap.CurrentSnapshot.GetLineFromLineNumber(node.Line - 1).Start + node.StartPos;
                        var span = new SnapshotSpan(bufferMap.CurrentSnapshot, start, node.EndPos - node.StartPos);
                        classificationSpans.Add(new ClassificationSpan(span,
                                                                       service.ClassificationTypeRegistry.GetClassificationType(
                                                                           node.Format)));
                    }
                }
            Errors = compileResult.Errors;
            Warnings = compileResult.Warnings;
        }

        /// <summary>
        /// Produces a list of nodes for a given location (in the Boo Compiler format) filtered by the provided filter
        /// </summary>
        /// <param name="loc">Location in the source code as provided by Boo compiler</param>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <remarks> The text span for the selected nodes include the location provided</remarks>
        public IEnumerable<MappedNode> GetNodes(LexicalInfo loc, Func<MappedNode, bool> filter)
        {
            if (bufferMap.FilePath == loc.FileName)
                return GetNodes(loc.Line - 1, bufferMap.MapPosition(loc.Line, loc.Column), filter);
            var source = service.GetSource(loc.FullPath) as BooSource;
            if (source == null)
                return null;
            return source.GetNodes(loc.Line - 1, source.MapPosition(loc.Line, loc.Column), filter);
        }

        /// <summary>
        /// Produces a list of nodes for a given location (in the text buffer coordinates) filtered by the provided filter
        /// </summary>
        /// <param name="line">The 0 based line number </param>
        /// <param name="pos">The 0 based position number </param>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <remarks> The text span for the selected nodes include the location provided</remarks>
        public IEnumerable<MappedNode> GetNodes(int line, int pos, Func<MappedNode, bool> filter)
        {
            return GetNodes(line, pos, (node, li, po) => (po >= node.StartPos && po <= node.EndPos), filter);
        }

        /// <summary>
        /// Produces a list of nodes adjacent to a given location (in the text buffer coordinates) filtered by the provided filter
        /// </summary>
        /// <param name="line"></param>
        /// <param name="pos"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <remarks> selects the closest nodes with the textspans ending before (to the left of) the location provided</remarks>
        internal IEnumerable<MappedNode> GetAdjacentNodes(int line, int pos, Func<MappedNode, bool> filter)
        {
            var list = GetNodes(line, pos, (node, li, po) => (po > node.EndPos), filter);
            var max = list.Max(item => item.EndPos);
            return list.Where(item => item.EndPos == max);
        }

        private IEnumerable<MappedNode> GetNodes(int line, int pos, Func<MappedNode, int, int, bool> comparer, Func<MappedNode, bool> filter)
        {
            List<MappedNode> nodes;
            if (!nodeDictionary.TryGetValue(line, out nodes))
                yield break;
            foreach (var node in nodes)
                if (filter(node) && comparer(node, line, pos))
                    yield return node;
        }

        internal SnapshotSpan GetSnapshotSpan(LexicalInfo lexicalInfo)
        {
            var line = bufferMap.CurrentSnapshot.GetLineFromLineNumber(lexicalInfo.Line - 1);
            var start = line.Start + bufferMap.MapPosition(lexicalInfo.Line, lexicalInfo.Column);
            var node = GetNodes(lexicalInfo, n => true).FirstOrDefault();
            if (node == null)
                return new SnapshotSpan(bufferMap.CurrentSnapshot, start, line.End - start);
            return new SnapshotSpan(bufferMap.CurrentSnapshot, start, node.Length);
        }

        internal IEnumerable<MappedNode> GetTypes()
        {
            foreach (var line in nodeDictionary.Values)
                foreach (var node in line)
                    if (node.Node is TypeDefinition)
                        yield return node;
        }

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
