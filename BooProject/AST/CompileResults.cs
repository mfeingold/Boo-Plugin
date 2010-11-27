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
using Boo.Lang.Parser;
using Hill30.BooProject.LanguageService;
using Boo.Lang.Compiler.IO;
using Microsoft.VisualStudio.Shell;
using CompilerParameters = Boo.Lang.Compiler.CompilerParameters;
using Boo.Lang.Compiler.Ast;
using Hill30.BooProject.AST.Nodes;
using Hill30.BooProject.Project;
using Microsoft.VisualStudio.Text.Classification;

namespace Hill30.BooProject.AST
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
        private readonly List<ClassificationSpan> classificationSpans = new List<ClassificationSpan>();

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
            if (line == -1)
                line = positionMap.Length;
            if (column == -1)
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
//            throw new NotImplementedException();
        }

        private void MapParsingMessage(CompilerError error)
        {
//            throw new NotImplementedException();
        }

        private void MapCompletedNodes(Module module)
        {
            new CompletedModuleWalker(this).Visit(module);
        }

        private void MapMessage(CompilerError error)
        {
//            throw new NotImplementedException();
        }

        private void MapMessage(CompilerWarning warning)
        {
//            throw new NotImplementedException();
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

        public IList<ClassificationSpan> ClassificationSpans { get { return classificationSpans; } }

        internal IEnumerable<MappedTypeDefinition> Types { get { return types; } }
    }
}
