using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boo.Lang.Compiler.Ast;

namespace Hill30.BooProject.LanguageService.NodeMapping
{
    public class MappedDeclarationNode : MappedNode
    {

        public MappedDeclarationNode(BufferMap bufferMap, ParameterDeclaration node)
            : base(bufferMap, node, 0)
        {
        }

        public MappedDeclarationNode(BufferMap bufferMap, Local node)
            : base(bufferMap, node, 0)
        {
        }

        public MappedDeclarationNode(BufferMap bufferMap, Field node)
            : base(bufferMap, node, 0)
        {
        }

        public override string QuickInfoTip
        {
            get { return null; }
        }
    }
}
