using Boo.Lang.Compiler.Ast;
using Hill30.BooProject.AST.Nodes;
using Hill30.BooProject.Compilation;

namespace Hill30.BooProject.AST.Walkers
{
    public class CompletedModuleWalker : DepthFirstVisitor
    {
        private readonly CompileResults result;
        public CompletedModuleWalker(CompileResults result)
        {
            this.result = result;
        }

        //public override void OnReferenceExpression(ReferenceExpression node)
        //{
        //    if (node.LexicalInfo != null)
        //        result.MapNode(RecordingStage.Completed, new MappedReferenceExpression(result, node));
        //    base.OnReferenceExpression(node);
        //}

        //public override void OnMemberReferenceExpression(MemberReferenceExpression node)
        //{
        //    if (node.LexicalInfo != null)
        //        result.MapNode(RecordingStage.Completed, new MappedReferenceExpression(result, node));
        //    base.OnMemberReferenceExpression(node);
        //}

        //protected override void OnError(Node node, System.Exception error)
        //{
        //    //base.OnError(node, error);
        //}
    }
}
