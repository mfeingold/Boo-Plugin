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
using Boo.Lang.Compiler.TypeSystem.Internal;
using Boo.Lang.Parser;
using Microsoft.VisualStudio.Shell;
using Boo.Lang.Compiler.Ast;
using Hill30.BooProject.AST.Nodes;
using Hill30.BooProject.Project;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Hill30.BooProject.LanguageService;

namespace Hill30.BooProject.AST
{
    /// <summary>
    /// The cached AST for a Boo source file
    /// </summary>
    public class CompileResults
    {
        private int[][] positionMap;
        private int[] lineMap;
        private int lineSize;
        private readonly List<MappedToken> tokenMap = new List<MappedToken>();
        private readonly List<MappedTypeDefinition> types = new List<MappedTypeDefinition>();
        private List<ClassificationSpan> classificationSpans;
        private readonly List<CompilerMessage> messages = new List<CompilerMessage>();
        private readonly BooFileNode fileNode;
        private readonly BooLanguageService service;

        public CompileResults(BooFileNode fileNode)
        {
            this.fileNode = fileNode;
            service = (BooLanguageService) fileNode.GetService(typeof (BooLanguageService));
        }

        private static antlr.IToken NextToken(antlr.TokenStream tokens)
        {
            while (true)
                try { return tokens.nextToken(); }
                // ReSharper disable EmptyGeneralCatchClause
                catch { }
                // ReSharper restore EmptyGeneralCatchClause
        }

        private void ExpandTabs(string source, int tabSize)
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
            lineMap = new int[positionMap.Length];
            lineMap[0] = 0;
            for (var i = 1; i < lineMap.Length; i++)
                lineMap[i] = lineMap[i - 1] + positionMap[i - 1].Length;
        }

        public void Initialize(string filePath, string source)
        {
            var tabSize = GlobalServices.LanguageService.GetLanguagePreferences().TabSize;
            ExpandTabs(source, tabSize);
            tokenMap.Clear();
            MapTokens(tabSize, source);
        }

        private void MapTokens(int tabSize, string source)
        {

            var tokens = BooParser.CreateBooLexer(tabSize, "code stream", new StringReader(source));

            antlr.IToken token;
            var currentPos = 0;

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
                var endIndex = positionMap[token.getLine() - 1][token.getColumn() - 1 + length];
                var startLine = token.getLine() - 1;

                var cluster = new MappedToken(
                    startLine * lineSize + startIndex,
                    endIndex - startIndex,
                    token);

                if (tokenMap.Count > 0
                    && tokenMap[tokenMap.Count() - 1].Index >= cluster.Index)
                    throw new ArgumentException("Token Mapping order");
                tokenMap.Add(cluster);

//                currentPos = tokenHandler(currentPos, token);
            }

//            tokenHandler(currentPos, token);
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
            if (column == -1 || column > positionMap[line-1].Length)
                column = positionMap[line - 1].Length;
            return new BufferPoint { Line = line - 1, Column = positionMap[line - 1][column - 1] };
        }

        internal BufferPoint LocationToPoint(SourceLocation location)
        {
            return LocationToPoint(location.Line, location.Column);
        }

        public bool IsDirty { get { return true; } }

        internal static void MapParsedNodes(Dictionary<string, Tuple<BooFileNode, CompileResults>> results, CompilerContext compilerContext)
        {
            foreach (var module in compilerContext.CompileUnit.Modules)
                results[module.LexicalInfo.FullPath].Item2.MapParsedNodes(module);

            foreach (var error in compilerContext.Errors)
                results[error.LexicalInfo.FullPath].Item2.MapParsingMessage(error);

            foreach (var warning in compilerContext.Warnings)
                results[warning.LexicalInfo.FullPath].Item2.MapParsingMessage(warning);
        }

        internal static void MapCompleted(Dictionary<string, Tuple<BooFileNode, CompileResults>> results, CompilerContext compilerContext)
        {
            foreach (var module in compilerContext.CompileUnit.Modules)
                results[module.LexicalInfo.FullPath].Item2.MapCompletedNodes(module);

            foreach (var error in compilerContext.Errors)
                results[error.LexicalInfo.FullPath].Item2.MapMessage(error);

            foreach (var warning in compilerContext.Warnings)
                results[warning.LexicalInfo.FullPath].Item2.MapMessage(warning);
        }

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

        private void MapParsedNodes(Module module)
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
            TokensForNode(node, cluster => node.Record(stage, cluster.Nodes));
        }

        private void MapParsingMessage(CompilerWarning warning)
        {
            MapParsingMessage(warning.LexicalInfo);
        }

        private void MapParsingMessage(CompilerError error)
        {
            MapParsingMessage(error.LexicalInfo);
        }

        private void MapParsingMessage(LexicalInfo location)
        {
            var token = GetMappedToken(location);
            if (token != null && token.Nodes
                .Where(n =>
                    n.Type != MappedNodeType.TypeDefiniton
                    && n.Type != MappedNodeType.TypeMemberDefinition
                    ).Count() == 0)
                MapParsedNode(new ParsingError(this, location, token.Length));
        }

        private void MapCompletedNodes(Module module)
        {
            new CompletedModuleWalker(this).Visit(module);
            foreach (var token in tokenMap)
                token.Nodes.ForEach(n => n.Resolve());
            var compileUnit = new CompileUnit();
            compileUnit.Modules.Add(module);
            CompileUnit = new InternalCompileUnit(compileUnit);
        }

        private void MapMessage(CompilerError error)
        {
            if (error.Code == "BCE0055")
                // I do not care about internal compiler errors here
                // In particular if an attribute type fails to resolve the CheckNeverUsedMethods step throws an exception
                return; 
            messages.Add(new CompilerMessage(fileNode, error.LexicalInfo, error.Code, error.Message, TaskErrorCategory.Error));
        }

        private void MapMessage(CompilerWarning warning)
        {
            messages.Add(new CompilerMessage(fileNode, warning.LexicalInfo, warning.Code, warning.Message, TaskErrorCategory.Warning));
        }

        internal MappedToken GetAdjacentMappedToken(int line, int column)
        {
            return tokenMap[Lookup(IndexOfBufferPoint(line, column))];
        }

        public MappedToken GetMappedToken(int line, int column)
        {
            var index = IndexOfBufferPoint(line, column);
            var token = tokenMap[Lookup(index)];
            if (token.Index + token.Length >= index)
                return token;
            return null;
        }

        public MappedToken GetMappedToken(SourceLocation location)
        {
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

        public void ShowMessages()
        {
            messages.ForEach(m => m.Show());
        }

        public void HideMessages()
        {
            messages.ForEach(m => m.Hide());
            // the spans have to be recalcualted becaise the next time we need them
            // the buffer will be different and the spans will no longer be valid
            classificationSpans = null;
        }

        internal IEnumerable<MappedTypeDefinition> Types { get { return types; } }

        internal IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            if (classificationSpans == null)
            {
                classificationSpans = new List<ClassificationSpan>();
                foreach (var token in tokenMap)
                    token.Nodes.ForEach(
                        node =>
                            {
                                if (node.Format != null)
                                    classificationSpans.Add(
                                        new ClassificationSpan(
                                            GetSnapshotSpan(span.Snapshot, node),
                                            service.ClassificationTypeRegistry.GetClassificationType(
                                                node.Format)));
                            }
                        );
            }
            return classificationSpans;
        }

        private SnapshotSpan GetSnapshotSpan(ITextSnapshot snapshot, MappedNode node)
        {
            var start = snapshot.GetLineFromLineNumber(node.TextSpan.iStartLine).Start + node.TextSpan.iStartIndex;
            var end = snapshot.GetLineFromLineNumber(node.TextSpan.iEndLine).Start + node.TextSpan.iEndIndex;
            return new SnapshotSpan(start, end - start);
        }

        private SnapshotSpan GetSnapshotSpan(ITextSnapshot snapshot, SourceLocation lexicalInfo)
        {
            var token = GetMappedToken(lexicalInfo);
            if (token != null && token.Nodes.Count > 0)
            {
                var textSpan = token.Nodes.LastOrDefault().TextSpan;
                var start = snapshot.GetLineFromLineNumber(textSpan.iStartLine).Start + textSpan.iStartIndex;
                return new SnapshotSpan(start, token.Length);
            }
            var pos = LocationToPoint(lexicalInfo);
            var line = snapshot.GetLineFromLineNumber(pos.Line);
            return new SnapshotSpan(line.Start, line.Length);
        }

        internal IEnumerable<ITagSpan<ErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (var task in messages)
            {
                yield return new TagSpan<ErrorTag>(
                    GetSnapshotSpan(spans[0].Snapshot, new SourceLocation(task.LexicalInfo.Line, task.LexicalInfo.Column)),
                    new ErrorTag(
                        task.ErrorCategory == TaskErrorCategory.Error ? PredefinedErrorTypeNames.SyntaxError : PredefinedErrorTypeNames.Warning,
                        task.Message
                        )
                    );
            }
        }

        public Boo.Lang.Compiler.TypeSystem.ICompileUnit CompileUnit { get; private set; }
    }
}
