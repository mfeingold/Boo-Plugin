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
using System.Linq;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Package;

namespace Hill30.Boo.ASTMapper.AST
{
    public class MappedToken : IComparable<MappedToken>
    {
        private readonly List<MappedNode> nodes = new List<MappedNode>();

        public MappedToken(int index)
        {
            Index = index;
        }

        public MappedToken(int index, int length)
        {
            Index = index;
            Length = length;
        }

        public int Index { get; private set; }
        public int Length { get; private set; }
        public List<MappedNode> Nodes { get { return nodes; } }

        #region IComparable<MappedToken> Members

        public int CompareTo(MappedToken other)
        {
            return Index.CompareTo(other.Index);
        }

        #endregion

        public string GetDataTiptext(out TextSpan span)
        {
            var tip = "";
            span = new TextSpan();
            foreach (var node in nodes.Where(node => node.QuickInfoTip != null))
            {
                span = span.Union(node.TextSpan);
                if (tip != "")
                    tip += '\n';
                tip += node.QuickInfoTip;
            }
            return tip;
        }

        public Declarations GetDeclarations(TokenInfo info, ParseReason reason)
        {
            var node = Nodes.Where(n => n.Declarations.GetCount() > 0).FirstOrDefault();
            if (node == null)
                return new BooDeclarations();
            return node.Declarations;
        }

        public string Goto(out TextSpan span)
        {
            var node = nodes.Where(n => n.DeclarationNode != null).FirstOrDefault();
            if (node != null)
            {
                span = node.DeclarationNode.TextSpan;
                return node.DeclarationNode.LexicalInfo.FullPath;
            }
            span = new TextSpan();
            return null;
        }
    }
}
