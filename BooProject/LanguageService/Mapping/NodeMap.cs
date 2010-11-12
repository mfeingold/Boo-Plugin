using System;
using System.Collections.Generic;
using System.Linq;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Parser;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text;
using Hill30.BooProject.LanguageService.Colorizer;
using Boo.Lang.Compiler;
using TypeDefinition = Hill30.BooProject.LanguageService.Mapping.Nodes.MappedTypeDefinition;

namespace Hill30.BooProject.LanguageService.Mapping
{
    public class NodeMap
    {
        private readonly Dictionary<int, List<MappedNode>> nodeDictionary = new Dictionary<int, List<MappedNode>>();
        private readonly List<ClassificationSpan> classificationSpans = new List<ClassificationSpan>();
        private readonly BooLanguageService service;
        private readonly BufferMap bufferMap;

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


        private antlr.IToken nextToken(antlr.TokenStream tokens)
        {
            while (true)
                try { return tokens.nextToken(); }
// ReSharper disable EmptyGeneralCatchClause
                catch {}
// ReSharper restore EmptyGeneralCatchClause
        }

        internal void MapTokens(antlr.TokenStream tokens)
        {
            antlr.IToken token;
            var currentPos = 0;

            while ((token = nextToken(tokens)).Type != BooLexer.EOF)
            {

                int length;

                switch (token.Type)
                {
                    case BooLexer.INDENT:
                    case BooLexer.DEDENT:
                    case BooLexer.EOL:
                        continue;
                    case BooLexer.SINGLE_QUOTED_STRING:
                    case BooLexer.DOUBLE_QUOTED_STRING:
                        length = token.getText().Length + 2;
                        break;
                    case BooLexer.TRIPLE_QUOTED_STRING:
                        length = token.getText().Length + 6;
                        break;
                    default:
                        length = token.getText().Length;
                        break;
                }

                var start = bufferMap.MapPosition(token.getLine(), token.getColumn());
                var end = bufferMap.MapPosition(token.getLine(), token.getColumn() + length);
                length = end.Column - start.Column;

                var span =
                    new SnapshotSpan(bufferMap.CurrentSnapshot,
                        bufferMap.CurrentSnapshot.GetLineFromLineNumber(token.getLine() - 1).Start + start.Column,
                        length);

                if (span.Start > currentPos)
                    classificationSpans.Add(new ClassificationSpan
                        (new SnapshotSpan(bufferMap.CurrentSnapshot, currentPos, span.Start - currentPos),
                        service.ClassificationTypeRegistry.GetClassificationType(Formats.BooBlockComment)
                        ));

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
            if (!nodeDictionary.TryGetValue(node.LexicalInfo.Line - 1, out nodes))
                nodeDictionary[node.LexicalInfo.Line - 1] = nodes = new List<MappedNode>();
            nodes.Add(node);
        }

        internal void Complete(CompilerContext compileResult)
        {
            foreach (var list in nodeDictionary.Values)
                foreach (var node in list)
                {
                    node.Resolve();
                    if (node.Format != null)
                    {
                        classificationSpans.Add(
                            new ClassificationSpan(
                                node.TextSpan.GetSnapshotSpan(bufferMap.CurrentSnapshot),
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
                return GetNodes(loc.Line - 1, bufferMap.MapPosition(loc.Line, loc.Column).Column, filter);
            var source = service.GetSource(loc.FullPath) as BooSource;
            if (source == null)
                return null;
            var mappedLoc = source.MapPosition(loc.Line, loc.Column);
            return source.GetNodes(mappedLoc.Line, mappedLoc.Column, filter);
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
            return GetNodes(line, pos, (node, li, po) => node.TextSpan.Contains(li, po), filter);
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
            var list = GetNodes(line, pos, (node, li, po) => (po > node.TextSpan.iEndIndex), filter);
            var max = list.Max(item => item.TextSpan.iEndIndex);
            return list.Where(item => item.TextSpan.iEndIndex == max);
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
            var start = line.Start + bufferMap.MapPosition(lexicalInfo.Line, lexicalInfo.Column).Column;
            var node = GetNodes(lexicalInfo, n => true).FirstOrDefault();
            if (node == null)
                return new SnapshotSpan(bufferMap.CurrentSnapshot, start, line.End - start);
            return node.TextSpan.GetSnapshotSpan(bufferMap.CurrentSnapshot);
        }

        internal IEnumerable<TypeDefinition> GetTypes()
        {
            foreach (var line in nodeDictionary.Values)
                foreach (var node in line)
                    if (node is TypeDefinition)
                        yield return (TypeDefinition)node;
        }

    }
}
