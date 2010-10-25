using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boo.Lang.Compiler.Ast;

namespace Hill30.BooProject.LanguageService.NodeMapping
{
    public class AstWalker : DepthFirstVisitor
    {
        private readonly Mapper mapper;
        public AstWalker(BooSource source, Mapper mapper)
        {
            this.source = source;
            this.mapper = mapper;
        }

        private readonly BooSource source;

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

        protected override void OnError(Node node, Exception error)
        {
            // Do Nothing
        }
    }

}
