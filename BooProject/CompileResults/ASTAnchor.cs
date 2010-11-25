using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Package;

namespace Hill30.BooProject.CachedAST
{
    public class ASTAnchor : IComparable<ASTAnchor>
    {
//        private readonly List<MappedNode> nodes = new List<MappedNode>();
        private antlr.IToken token;

        public ASTAnchor(int index)
        {
            Index = index;
        }

        public ASTAnchor(int index, int length, antlr.IToken token)
        {
            Index = index;
            Length = length;
            this.token = token;
        }

        public int Index { get; private set; }
        public int Length { get; private set; }
//        public List<MappedNode> Nodes { get { return nodes; } }

        //public void Resolve(Action<MappedNode> process)
        //{
        //    foreach (var node in Nodes)
        //    {
        //        node.Resolve();
        //        process(node);
        //    }

        //}

        #region IComparable<ASTAnchor> Members

        public int CompareTo(ASTAnchor other)
        {
            return Index.CompareTo(other.Index);
        }

        #endregion


        //internal string GetDataTiptext(out TextSpan span)
        //{
        //    var tip = "";
        //    span = new TextSpan();
        //    foreach (var node in nodes.Where(node => node.QuickInfoTip != null))
        //    {
        //        span = span.Union(node.TextSpan);
        //        if (tip != "")
        //            tip += '\n';
        //        tip += node.QuickInfoTip;
        //    }
        //    return tip;
        //}

        //internal Declarations GetDeclarations(TokenInfo info, ParseReason reason)
        //{
        //    var node = Nodes.Where(n => n.Declarations.GetCount() > 0).FirstOrDefault();
        //    if (node == null)
        //        return new BooDeclarations();
        //    return node.Declarations;
        //}

        //internal string Goto(out TextSpan span)
        //{
        //    var node = nodes.Where(n => n.DeclarationNode != null).FirstOrDefault();
        //    if (node != null)
        //    {
        //        span = node.DeclarationNode.TextSpan;
        //        return node.DeclarationNode.LexicalInfo.FullPath;
        //    }
        //    span = new TextSpan();
        //    return null;
        //}
    }
}
