using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boo.Lang.Compiler.Ast;
using Hill30.BooProject.AST.Nodes;

namespace Hill30.BooProject.AST
{
    public class CompletedModuleWalker : DepthFirstVisitor
    {
        private CompileResults result;
        public CompletedModuleWalker(CompileResults result)
        {
            this.result = result;
        }

        public override void OnMemberReferenceExpression(MemberReferenceExpression node)
        {
            if (node.LexicalInfo != null)
                result.MapNode(RecordingStage.Completed, new MappedReferenceExpression(result, node));
            base.OnMemberReferenceExpression(node);
        }

        protected override void OnError(Node node, System.Exception error)
        {
            //base.OnError(node, error);
        }
    }
}
