using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.BooProject.LanguageService.Mapping
{
    public class NodeCluster : IComparable<NodeCluster>
    {
        private readonly List<MappedNode> nodes = new List<MappedNode>();
        private antlr.IToken token;

        public NodeCluster(int index)
        {
            Index = index;
        }

        public NodeCluster(int lineSize, BufferMap.BufferPoint start, int length, antlr.IToken token)
        {
            Index = start.Line * lineSize + start.Column;
            Length = length;
            this.token = token;
        }

        public int Index { get; private set; }
        public int Length { get; private set; }
        public List<MappedNode> Nodes { get { return nodes; } }

        public void Resolve(Action<MappedNode> process)
        {
            foreach (var node in Nodes)
            {
                node.Resolve();
                process(node);
            }

        }

        #region IComparable<NodeBundle> Members

        public int CompareTo(NodeCluster other)
        {
            return Index.CompareTo(other.Index);
        }

        #endregion


        internal string GetDataTiptext(out TextSpan span)
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

        internal Declarations GetDeclarations(TokenInfo info, ParseReason reason)
        {
            var node = Nodes.Where(n => n.Declarations.GetCount() > 0).FirstOrDefault();
            if (node == null)
                return new BooDeclarations();
            return node.Declarations;
        }

        internal string Goto(out TextSpan span)
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
