using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.TypeSystem;

namespace Hill30.BooProject.LanguageService.NodeMapping
{
    public class ParsedAstWalker : DepthFirstVisitor
    {
        private readonly Mapper mapper;
 
        public ParsedAstWalker(Mapper mapper)
        {
            this.mapper = mapper;
        }

        public override void OnParameterDeclaration(ParameterDeclaration node)
        {
            if (node.LexicalInfo != null)
                mapper.MapNode(new MappedDeclarationNode(mapper, node));
            base.OnParameterDeclaration(node);
        }

        public override void OnLocal(Local node)
        {
            if (node.LexicalInfo != null)
                mapper.MapNode(new MappedDeclarationNode(mapper, node));
            base.OnLocal(node);
        }

        public override void OnField(Field node)
        {
            if (node.LexicalInfo != null)
                mapper.MapNode(new MappedDeclarationNode(mapper, node));
            base.OnField(node);
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

        public override void OnMemberReferenceExpression(MemberReferenceExpression node)
        {
            if (node.LexicalInfo != null)
                mapper.MapNode(new MappedReferenceNode(mapper, node));
            base.OnMemberReferenceExpression(node);
        }

        public override void OnMacroStatement(MacroStatement node)
        {
            if (node.LexicalInfo != null)
                mapper.MapNode(new MappedMacroNode(mapper, node));
            base.OnMacroStatement(node);
        }

        protected override void OnError(Node node, Exception error)
        {
            // Do Nothing
        }
    }

}
