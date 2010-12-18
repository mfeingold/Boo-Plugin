using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boo.Lang.Compiler.Ast;

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
