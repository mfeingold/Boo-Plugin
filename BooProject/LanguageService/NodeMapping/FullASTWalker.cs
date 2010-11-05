using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boo.Lang.Compiler.Ast;

namespace Hill30.BooProject.LanguageService.NodeMapping
{
    public class FullAstWalker : DepthFirstVisitor
    {
        private readonly Mapper mapper;

        public FullAstWalker(Mapper mapper)
        {
            this.mapper = mapper;
        }

        public override void OnMemberReferenceExpression(MemberReferenceExpression node)
        {
            if (node.LexicalInfo != null)
                mapper.MapNode(new MappedReferenceNode(mapper, node));
            base.OnMemberReferenceExpression(node);
        }

        protected override void OnError(Node node, Exception error)
        {
            base.OnError(node, error);
        }
    }
}
