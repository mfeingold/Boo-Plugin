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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.IO;
using Boo.Lang.Compiler.TypeSystem;
using Boo.Lang.Compiler.TypeSystem.Internal;
using Boo.Lang.Parser;
using Hill30.Boo.ASTMapper.AST;
using Hill30.Boo.ASTMapper.AST.Nodes;
using Hill30.Boo.ASTMapper.AST.Walkers;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.Boo.ASTMapper
{
    /// <summary>
    /// The cached AST for a Boo source file
    /// </summary>
    public class CompileResults
    {
        private int[][] positionMap;
        private int lineSize;
        private readonly List<MappedToken> tokenMap = new List<MappedToken>();
        private readonly List<MappedTypeDefinition> types = new List<MappedTypeDefinition>();
        private readonly List<CompilerMessage> messages = new List<CompilerMessage>();
        private readonly List<TextSpan> whitespaces = new List<TextSpan>();
        private readonly Func<string> urlGetter;
        private readonly Func<string> sourceGetter;
        private readonly Func<int> tabsizeGetter;

        public CompileResults(Func<string> urlGetter, Func<string> sourceGetter, Func<int> tabsizeGetter)
        {
            this.urlGetter = urlGetter;
            this.sourceGetter = sourceGetter;
            this.tabsizeGetter = tabsizeGetter;
        }

        private static antlr.IToken NextToken(antlr.TokenStream tokens)
        {
            while (true)
                try { return tokens.nextToken(); }
                // ReSharper disable EmptyGeneralCatchClause
                catch { }
            // ReSharper restore EmptyGeneralCatchClause
        }

        private void ExpandTabs(IEnumerable<char> source, int tabSize)
        {
            var sourcePos = 0;
            var mappedPos = 0;
            var mappings = new List<int[]>();
            var positionList = new List<int>();
            lineSize = 0;
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
                if (c == '\n')
                {
                    lineSize = Math.Max(lineSize, mappedPos);
                    mappings.Add(positionList.ToArray());
                    positionList.Clear();
                    sourcePos = 0;
                    mappedPos = 0;
                }
            }
            positionList.Add(sourcePos); // to map the <EOL> token
            mappings.Add(positionList.ToArray());
            positionMap = mappings.ToArray();
        }

        private void Initialize()
        {
            var source = sourceGetter();
            var tabSize = tabsizeGetter();
            ExpandTabs(source, tabSize);
            whitespaces.Clear();
            tokenMap.Clear();
            if (source.Trim().Length > 0)
                MapTokens(tabSize, source);
        }

        private void MapTokens(int tabSize, string source)
        {

            var endLine = 0;
            var endIndex = 0;

            var tokens = BooParser.CreateBooLexer(tabSize, "code stream", new StringReader(source));

            antlr.IToken token;
            while ((token = NextToken(tokens)).Type != BooLexer.EOF)
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

                var startIndex = positionMap[token.getLine() - 1][token.getColumn() - 1];
                var startLine = token.getLine() - 1;

                if (startLine > endLine || startLine == endLine && startIndex > endIndex)
                    whitespaces.Add(new TextSpan { iStartLine = endLine, iStartIndex = endIndex, iEndLine = startLine, iEndIndex = startIndex });

                endIndex = positionMap[token.getLine() - 1][token.getColumn() - 1 + length];
                endLine = startLine;

                var cluster = new MappedToken(
                    startLine * lineSize + startIndex,
                    endIndex - startIndex);

                if (tokenMap.Count > 0
                    && tokenMap[tokenMap.Count() - 1].Index >= cluster.Index)
                    throw new ArgumentException("Token Mapping order");
                tokenMap.Add(cluster);
            }

            var sIndex = positionMap[token.getLine() - 1][token.getColumn() - 1];
            var sLine = token.getLine() - 1;

            if (sLine > endLine || sLine == endLine && sIndex > endIndex)
                whitespaces.Add(new TextSpan { iStartLine = endLine, iStartIndex = endIndex, iEndLine = sLine, iEndIndex = sIndex });
        }

        public struct BufferPoint
        {
            public int Line;
            public int Column;
        }

        public BufferPoint LocationToPoint(int line, int column)
        {
            if (line == -1 || line > positionMap.Length)
                line = positionMap.Length;
            if (column == -1 || column > positionMap[line - 1].Length)
                column = positionMap[line - 1].Length;
            return new BufferPoint { Line = line - 1, Column = positionMap[line - 1][column - 1] };
        }

        internal BufferPoint LocationToPoint(SourceLocation location)
        {
            return LocationToPoint(location.Line, location.Column);
        }

        public bool IsDirty { get { return true; } }

        private int Lookup(int index)
        {
            var i = tokenMap.BinarySearch(new MappedToken(index));
            if (i >= 0)
                return i;
            return Math.Max(0, (~i) - 1);
        }

        private int IndexOfBufferPoint(int line, int column)
        {
            return line * lineSize + column;
        }

        private void TokensForNode(MappedNode node, Action<MappedToken> action)
        {
            var startIndex = IndexOfBufferPoint(node.TextSpan.iStartLine, node.TextSpan.iStartIndex);
            var endIndex = IndexOfBufferPoint(node.TextSpan.iEndLine, node.TextSpan.iEndIndex);
            for (var i = Lookup(startIndex); i < tokenMap.Count && tokenMap[i].Index + tokenMap[i].Length <= endIndex; i++)
                action(tokenMap[i]);
        }

        public void MapParsedNodes(Module module)
        {
            new ParsedModuleWalker(this).Visit(module);
        }

        internal void MapParsedNode(MappedNode node)
        {
            if (node.Type == MappedNodeType.TypeDefiniton)
                types.Add((MappedTypeDefinition)node);
            MapNode(RecordingStage.Parsed, node);
        }

        internal void MapNode(RecordingStage stage, MappedNode node)
        {
            TokensForNode(node, token => node.Record(stage, token));
        }

        public void MapParsingMessage(CompilerWarning warning)
        {
            MapParsingMessage(warning.LexicalInfo);
        }

        public void MapParsingMessage(CompilerError error)
        {
            MapParsingMessage(error.LexicalInfo);
        }

        public void MapParsingMessage(LexicalInfo location)
        {
            var token = GetMappedToken(location);
            if (token != null && token.Nodes
                .Where(n =>
                    n.Type != MappedNodeType.TypeDefiniton
                    && n.Type != MappedNodeType.TypeMemberDefinition
                    ).Count() == 0)
                MapParsedNode(new ParsingError(this, location, token.Length));
        }

        public void MapCompletedNodes(Module module)
        {
            new CompletedModuleWalker(this).Visit(module);
            foreach (var token in tokenMap)
// ReSharper disable AccessToModifiedClosure
                token.Nodes.ForEach(n => n.Resolve(token));
// ReSharper restore AccessToModifiedClosure
            var compileUnit = new CompileUnit();
            compileUnit.Modules.Add(module);
            CompileUnit = new InternalCompileUnit(compileUnit);
        }

        public void MapMessage(CompilerError error)
        {
            if (error.Code == "BCE0055")
                // I do not care about internal compiler errors here
                // In particular if an attribute type fails to resolve the CheckNeverUsedMethods step throws an exception
                return;
            messages.Add(new CompilerMessage(error.LexicalInfo, error.Code, error.Message, TaskErrorCategory.Error));
        }

        public void MapMessage(CompilerWarning warning)
        {
            messages.Add(new CompilerMessage(warning.LexicalInfo, warning.Code, warning.Message, TaskErrorCategory.Warning));
        }

        public MappedToken GetAdjacentMappedToken(int line, int column)
        {
            if (tokenMap.Count == 0)
                return null;
            return tokenMap[Lookup(IndexOfBufferPoint(line, column))];
        }

        public MappedToken GetMappedToken(int line, int column)
        {
            if (tokenMap.Count == 0)
                return null;
            var index = IndexOfBufferPoint(line, column);
            var token = tokenMap[Lookup(index)];
            if (token.Index + token.Length >= index)
                return token;
            return null;
        }

        public MappedToken GetMappedToken(SourceLocation location)
        {
            if (tokenMap.Count == 0)
                return null;
            var bufferPoint = LocationToPoint(location);
            return GetMappedToken(bufferPoint.Line, bufferPoint.Column);
        }

        public MappedNode GetMappedNode(Node node)
        {
            var token = GetMappedToken(node.LexicalInfo);
            if (token == null)
                return null;
            return token.Nodes.Where(n => n.Node == node).FirstOrDefault();
        }

        public void ShowMessages(Action<ErrorTask> exposer, Action<ErrorTask> navigator)
        {
            messages.ForEach(m => m.Show(exposer, navigator));
        }

        public void HideMessages(Action<ErrorTask> hider)
        {
            messages.ForEach(m => m.Hide(hider));
        }

        public IEnumerable<MappedTypeDefinition> Types { get { return types; } }

        private SnapshotSpan GetSnapshotSpan(ITextSnapshot snapshot, SourceLocation lexicalInfo, Func<TextSpan, SnapshotSpan> snapshotCreator)
        {
            var token = GetMappedToken(lexicalInfo);
            if (token != null && token.Nodes.Count > 0)
                return snapshotCreator(token.Nodes.Last().TextSpan).TranslateTo(snapshot, SpanTrackingMode.EdgeNegative);

            var pos = LocationToPoint(lexicalInfo);
            return snapshotCreator(
                new TextSpan
                    {
                        iStartLine = pos.Line,
                        iStartIndex = pos.Column,
                        iEndLine = pos.Line,
                        iEndIndex = -1
                    }
                ).TranslateTo(snapshot, SpanTrackingMode.EdgeNegative);
        }

        public IList<ClassificationSpan> GetClassificationSpans(IClassificationTypeRegistryService classificationRegistry, SnapshotSpan span, Func<TextSpan, SnapshotSpan> snapshotCreator)
        {
            var classificationSpans =
                whitespaces.Select(
                    whitespace =>
                        new ClassificationSpan(
                            snapshotCreator(whitespace).TranslateTo(span.Snapshot, SpanTrackingMode.EdgeNegative),
                            classificationRegistry.GetClassificationType(Formats.BooBlockComment)
                            )
                        ).ToList();

            foreach (var token in tokenMap)
                token.Nodes.ForEach(
                    node =>
                    {
                        if (node.Format != null)
                            classificationSpans.Add(
                                new ClassificationSpan(
                                    snapshotCreator(node.TextSpan).TranslateTo(span.Snapshot, SpanTrackingMode.EdgeNegative),
                                    classificationRegistry.GetClassificationType(
                                        node.Format)));
                    }
                    );

            return classificationSpans;
        }

        public IEnumerable<ITagSpan<ErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans, Func<TextSpan, SnapshotSpan> snapshotCreator)
        {
            if (spans.Count == 0)
                yield break;

            foreach (var task in messages)
            {
                yield return new TagSpan<ErrorTag>(
                    GetSnapshotSpan(spans[0].Snapshot, new SourceLocation(task.LexicalInfo.Line, task.LexicalInfo.Column), snapshotCreator),
                    new ErrorTag(
                        task.ErrorCategory == TaskErrorCategory.Error ? PredefinedErrorTypeNames.SyntaxError : PredefinedErrorTypeNames.Warning,
                        task.Message
                        )
                    );
            }
        }

        public ICompileUnit CompileUnit { get; private set; }

        public string Url { get { return urlGetter(); } }

        public void SetupForCompilation(CompilerParameters compilerParameters)
        {
                if (CompileUnit == null)
                    compilerParameters.Input.Add(new StringInput(urlGetter(), sourceGetter()));
                else
                    compilerParameters.References.Add(CompileUnit);
            Initialize();
        }
    }

}
