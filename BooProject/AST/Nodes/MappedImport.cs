using Boo.Lang.Compiler.Ast;
using Hill30.BooProject.Compilation;

namespace Hill30.BooProject.AST.Nodes
{
    public class MappedImport : MappedNode
    {
        public MappedImport(CompileResults results, Import node)
            : base(results, node, node.Namespace.Length)
        {
        }

        public override MappedNodeType Type
        {
            get { return MappedNodeType.Import; }
        }
    }
}
