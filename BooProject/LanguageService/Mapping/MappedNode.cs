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

using Boo.Lang.Compiler.Ast;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.BooProject.LanguageService.Mapping
{
    public enum MappedNodeType
    {
        MacroReference,
        TypeReference,
        TypeDefiniton,
        VraiableReference,
        VariableDefinition,
        TypeMemberDefinition,
    }

    public abstract class MappedNode
    {
        protected MappedNode(BufferMap bufferMap, LexicalInfo lexicalInfo, int length)
            : this(
                bufferMap.MapPosition(lexicalInfo.Line, lexicalInfo.Column),
                bufferMap.MapPosition(lexicalInfo.Line, lexicalInfo.Column + length)
            )
        {
            LexicalInfo = lexicalInfo;
        }

        protected MappedNode(BufferMap bufferMap, Node node)
            : this(
                bufferMap.MapPosition(node.LexicalInfo.Line, node.LexicalInfo.Column),
                bufferMap.MapPosition(node.EndSourceLocation.Line, node.EndSourceLocation.Column)
            )
        {
            LexicalInfo = node.LexicalInfo;
        }

        private MappedNode(BufferMap.BufferPoint start, BufferMap.BufferPoint end)
        {
            TextSpan = new TextSpan
            {
                iStartLine = start.Line,
                iStartIndex = start.Column,
                iEndLine = end.Line,
                iEndIndex = end.Column
            };
        }

        public LexicalInfo LexicalInfo { get; private set; }
        public TextSpan TextSpan { get; private set; }
        public virtual string QuickInfoTip { get { return null; } }
        public virtual string Format { get { return null; } }
        public virtual BooDeclarations Declarations { get { return new BooDeclarations(); } }
        internal protected virtual void Resolve() { }
        internal protected virtual MappedNode DeclarationNode { get { return null; } }
        public abstract MappedNodeType Type { get; }

    }
}
