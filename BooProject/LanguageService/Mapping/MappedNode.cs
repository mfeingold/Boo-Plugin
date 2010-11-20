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

using System.Collections.Generic;
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

    public enum RecordingStage
    {
        Parsed,
        Completed
    }

    public abstract class MappedNode
    {
        private bool resolved;

        protected MappedNode(BufferMap bufferMap, Node node, int length)
            : this(node,
                bufferMap.LocationToPoint(node.LexicalInfo),
                bufferMap.LocationToPoint(node.LexicalInfo.Line, node.LexicalInfo.Column + length)
            )
        {
            LexicalInfo = node.LexicalInfo;
        }

        protected MappedNode(BufferMap bufferMap, Node node)
            : this(node,
                bufferMap.LocationToPoint(node.LexicalInfo),
                bufferMap.LocationToPoint(node.EndSourceLocation)
            )
        {
            LexicalInfo = node.LexicalInfo;
        }

        private MappedNode(Node node, BufferMap.BufferPoint start, BufferMap.BufferPoint end)
        {
            Node = node;
            TextSpan = new TextSpan
            {
                iStartLine = start.Line,
                iStartIndex = start.Column,
                iEndLine = end.Line,
                iEndIndex = end.Column
            };
        }

        public Node Node { get; private set; }
        public LexicalInfo LexicalInfo { get; private set; }
        public TextSpan TextSpan { get; private set; }
        public virtual string QuickInfoTip { get { return null; } }
        public virtual string Format { get { return null; } }
        public virtual BooDeclarations Declarations { get { return new BooDeclarations(); } }
        protected virtual void ResolveImpl() { }
        internal protected virtual MappedNode DeclarationNode { get { return null; } }
        public abstract MappedNodeType Type { get; }

        internal void Resolve()
        {
            if (!resolved)
                ResolveImpl();
            resolved = true;
        }

        internal virtual void Record(RecordingStage stage, List<MappedNode> list)
        {
            list.Add(this);
        }
    }
}
