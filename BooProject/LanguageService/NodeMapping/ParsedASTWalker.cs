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
        private NodeMap nodeMap;
        private BufferMap bufferMap;
 
        public ParsedAstWalker(NodeMap nodeMap, BufferMap bufferMap)
        {
            this.nodeMap = nodeMap;
            this.bufferMap = bufferMap;
        }

        public override void OnParameterDeclaration(ParameterDeclaration node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedDeclarationNode(bufferMap, node));
            base.OnParameterDeclaration(node);
        }

        public override void OnLocal(Local node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedDeclarationNode(bufferMap, node));
            base.OnLocal(node);
        }

        public override void OnField(Field node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedDeclarationNode(bufferMap, node));
            base.OnField(node);
        }

        public override void OnClassDefinition(ClassDefinition node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedTypeNode(bufferMap, node));
            base.OnClassDefinition(node);
        }

        public override void OnSimpleTypeReference(SimpleTypeReference node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedTypeNode(bufferMap, node));
            base.OnSimpleTypeReference(node);
        }

        public override void OnReferenceExpression(ReferenceExpression node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedReferenceNode(nodeMap, bufferMap, node));
            base.OnReferenceExpression(node);
        }

        public override void OnSelfLiteralExpression(SelfLiteralExpression node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedReferenceNode(nodeMap, bufferMap, node));
            base.OnSelfLiteralExpression(node);
        }

        public override void OnMemberReferenceExpression(MemberReferenceExpression node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedReferenceNode(nodeMap, bufferMap, node));
            base.OnMemberReferenceExpression(node);
        }

        public override void OnMacroStatement(MacroStatement node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedMacroNode(bufferMap, node));
            base.OnMacroStatement(node);
        }

        protected override void OnError(Node node, Exception error)
        {
            // Do Nothing
        }
    }

}
