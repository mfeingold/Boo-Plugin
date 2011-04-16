using Boo.Lang.Compiler.Ast;
using Hill30.Boo.ASTMapper.AST.Nodes;

namespace Hill30.Boo.ASTMapper.AST.Walkers
{
    public class CompletedModuleWalker : DepthFirstVisitor
    {
        private readonly CompileResults results;
        public CompletedModuleWalker(CompileResults results)
        {
            this.results = results;
        }

        public override void OnLocal(Local node)
        {
            if (node.LexicalInfo != null)
                results.MapParsedNode(new MappedVariableDefinition(results, node));
            base.OnLocal(node);
        }

        public override void OnMemberReferenceExpression(MemberReferenceExpression node)
        {
            if (node.LexicalInfo != null)
                results.MapNode(RecordingStage.Completed, new MappedReferenceExpression(results, node));
            base.OnReferenceExpression(node);
        }
    }
}
