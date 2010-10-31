using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.TypeSystem;

namespace Hill30.BooProject.LanguageService.NodeMapping
{
    public class AstWalker : DepthFirstVisitor
    {
        private readonly Mapper mapper;
 
        public AstWalker(Mapper mapper)
        {
            this.mapper = mapper;
        }

        public override void OnClassDefinition(ClassDefinition node)
        {
            if (node.LexicalInfo != null)
                mapper.MapNode(new MappedTypeNode(mapper, node));
            base.OnClassDefinition(node);
        }

        public override void OnSimpleTypeReference(SimpleTypeReference node)
        {
            if (node.LexicalInfo != null)
                mapper.MapNode(new MappedTypeNode(mapper, node));
            base.OnSimpleTypeReference(node);
        }

        public override void OnReferenceExpression(ReferenceExpression node)
        {
            if (node.LexicalInfo != null)
                mapper.MapNode(new MappedReferenceNode(mapper, node));
            base.OnReferenceExpression(node);
        }

        protected override void OnError(Node node, Exception error)
        {
            // Do Nothing
        }
    }

}
