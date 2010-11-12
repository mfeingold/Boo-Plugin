using Boo.Lang.Compiler.Ast;
using Hill30.BooProject.LanguageService.Mapping.Nodes;

namespace Hill30.BooProject.LanguageService.Mapping
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
                nodeMap.MapNode(new MappedReferenceExpression(nodeMap, bufferMap, node));
            base.OnMemberReferenceExpression(node);
        }

        protected override void OnError(Node node, System.Exception error)
        {
            //base.OnError(node, error);
        }
    }
}
