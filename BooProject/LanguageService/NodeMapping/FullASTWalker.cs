using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boo.Lang.Compiler.Ast;

namespace Hill30.BooProject.LanguageService.NodeMapping
{
    public class FullAstWalker : DepthFirstVisitor
    {
        private readonly NodeMap nodeMap;
        private readonly BufferMap bufferMap;

        public FullAstWalker(NodeMap nodeMap, BufferMap bufferMap)
        {
            this.nodeMap = nodeMap;
            this.bufferMap = bufferMap;
        }

        public override void OnMemberReferenceExpression(MemberReferenceExpression node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedReferenceNode(nodeMap, bufferMap, node));
            base.OnMemberReferenceExpression(node);
        }

        protected override void OnError(Node node, Exception error)
        {
            base.OnError(node, error);
        }
    }
}
