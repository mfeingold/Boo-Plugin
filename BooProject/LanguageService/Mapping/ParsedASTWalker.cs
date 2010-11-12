using System;
using Boo.Lang.Compiler.Ast;
using Hill30.BooProject.LanguageService.Mapping.Nodes;

namespace Hill30.BooProject.LanguageService.Mapping
{
    public class ParsedAstWalker : DepthFirstVisitor
    {
        private readonly NodeMap nodeMap;
        private readonly BufferMap bufferMap;
 
        public ParsedAstWalker(NodeMap nodeMap, BufferMap bufferMap)
        {
            this.nodeMap = nodeMap;
            this.bufferMap = bufferMap;
        }

        public override void OnParameterDeclaration(ParameterDeclaration node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedVariableDefinition(bufferMap, node));
            base.OnParameterDeclaration(node);
        }

        public override void OnLocal(Local node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedVariableDefinition(bufferMap, node));
            base.OnLocal(node);
        }

        public override void OnField(Field node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedVariableDefinition(bufferMap, node));
            base.OnField(node);
        }

        public override void OnClassDefinition(ClassDefinition node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedTypeDefinition(bufferMap, node));
            base.OnClassDefinition(node);
        }

        public override void OnModule(Module node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedTypeDefinition(bufferMap, node));
            base.OnModule(node);
        }

        public override void OnSimpleTypeReference(SimpleTypeReference node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedTypeReference(bufferMap, node));
            base.OnSimpleTypeReference(node);
        }

        public override void OnReferenceExpression(ReferenceExpression node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedReferenceExpression(nodeMap, bufferMap, node));
            base.OnReferenceExpression(node);
        }

        public override void OnSelfLiteralExpression(SelfLiteralExpression node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedReferenceExpression(nodeMap, bufferMap, node));
            base.OnSelfLiteralExpression(node);
        }

        public override void OnMemberReferenceExpression(MemberReferenceExpression node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedReferenceExpression(nodeMap, bufferMap, node));
            base.OnMemberReferenceExpression(node);
        }

        public override void OnMacroStatement(MacroStatement node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedMacroReference(bufferMap, node));
            base.OnMacroStatement(node);
        }

        protected override void OnError(Node node, Exception error)
        {
            // Do Nothing
        }
    }

}
