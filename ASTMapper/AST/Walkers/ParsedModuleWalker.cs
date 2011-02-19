using System;
using Boo.Lang.Compiler.Ast;
using Hill30.Boo.ASTMapper.AST.Nodes;

namespace Hill30.Boo.ASTMapper.AST.Walkers
{
    public class ParsedModuleWalker : DepthFirstVisitor
    {
        private readonly CompileResults results;
        
        public ParsedModuleWalker(CompileResults results)
        {
            this.results = results;
        }

        public override void OnParameterDeclaration(ParameterDeclaration node)
        {
            if (node.LexicalInfo != null)
                results.MapParsedNode(new MappedVariableDefinition(results, node));
            base.OnParameterDeclaration(node);
        }

        public override void OnLocal(Local node)
        {
            if (node.LexicalInfo != null)
                results.MapParsedNode(new MappedVariableDefinition(results, node));
            base.OnLocal(node);
        }

        public override void OnField(Field node)
        {
            if (node.LexicalInfo != null)
                results.MapParsedNode(new MappedTypeMemberDefinition(results, node));
            base.OnField(node);
        }

        public override void OnProperty(Property node)
        {
            if (node.LexicalInfo != null)
                results.MapParsedNode(new MappedTypeMemberDefinition(results, node));
            base.OnProperty(node);
        }

        public override void OnMethod(Method node)
        {
            if (node.LexicalInfo != null)
                results.MapParsedNode(new MappedTypeMemberDefinition(results, node));
            base.OnMethod(node);
        }

        public override void OnEvent(Event node)
        {
            if (node.LexicalInfo != null)
                results.MapParsedNode(new MappedTypeMemberDefinition(results, node));
            base.OnEvent(node);
        }

        public override void OnClassDefinition(ClassDefinition node)
        {
            if (node.LexicalInfo != null)
                results.MapParsedNode(new MappedTypeDefinition(results, node));
            base.OnClassDefinition(node);
        }

        public override void OnModule(Module node)
        {
            if (node.LexicalInfo != null)
                results.MapParsedNode(new MappedTypeDefinition(results, node));
            base.OnModule(node);
        }

        public override void OnSimpleTypeReference(SimpleTypeReference node)
        {
            if (node.LexicalInfo != null)
                results.MapParsedNode(new MappedTypeReference(results, node));
            base.OnSimpleTypeReference(node);
        }

        public override void OnGenericTypeReference(GenericTypeReference node)
        {
            if (node.LexicalInfo != null)
                results.MapParsedNode(new MappedTypeReference(results, node));
            base.OnGenericTypeReference(node);
        }

        public override void OnReferenceExpression(ReferenceExpression node)
        {
            if (node.LexicalInfo != null)
                results.MapParsedNode(new MappedReferenceExpression(results, node));
            base.OnReferenceExpression(node);
        }

        public override void OnSelfLiteralExpression(SelfLiteralExpression node)
        {
            if (node.LexicalInfo != null)
                results.MapParsedNode(new MappedReferenceExpression(results, node));
            base.OnSelfLiteralExpression(node);
        }

        public override void OnMemberReferenceExpression(MemberReferenceExpression node)
        {
            if (node.LexicalInfo != null)
                results.MapParsedNode(new MappedReferenceExpression(results, node));
            base.OnMemberReferenceExpression(node);
        }

        public override void OnMacroStatement(MacroStatement node)
        {
            if (node.LexicalInfo != null)
                results.MapParsedNode(new MappedMacroReference(results, node));
            base.OnMacroStatement(node);
        }

        protected override void OnError(Node node, Exception error)
        {
            // Do Nothing
        }

        public override void OnAttribute(global::Boo.Lang.Compiler.Ast.Attribute node)
        {
            if (node.LexicalInfo != null)
                results.MapParsedNode(new MappedAttribute(results, node));
            base.OnAttribute(node);
        }

        public override void OnImport(Import node)
        {
            if (node.LexicalInfo != null)
                results.MapParsedNode(new MappedImport(results, node));
            base.OnImport(node);
        }
    }
}
