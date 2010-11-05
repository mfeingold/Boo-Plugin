using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boo.Lang.Compiler.Ast;

namespace Hill30.BooProject.LanguageService.NodeMapping
{
    public class MappedDeclarationNode : MappedNode
    {

        public MappedDeclarationNode(Mapper mapper, ParameterDeclaration node)
            : base(mapper, node, 0)
        {
        }

        public MappedDeclarationNode(Mapper mapper, Local node)
            : base(mapper, node, 0)
        {
        }

        public MappedDeclarationNode(Mapper mapper, Field node)
            : base(mapper, node, 0)
        {
        }

        public override string QuickInfoTip
        {
            get { return null; }
        }
    }
}
