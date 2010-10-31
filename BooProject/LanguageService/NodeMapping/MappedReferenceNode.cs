using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boo.Lang.Compiler.Ast;

namespace Hill30.BooProject.LanguageService.NodeMapping
{
    public class MappedReferenceNode : MappedNode
    {
        private Mapper mapper;
        private readonly string quickInfoTip;

        public MappedReferenceNode(Mapper mapper, ReferenceExpression node)
            : base(mapper, node.LexicalInfo.Line, node.LexicalInfo.Column, node.Name.Length)
        {
            this.mapper = mapper;
            quickInfoTip = "(var) " + node.Name + " as " + node.ExpressionType.FullName;
        }

        public override string QuickInfoTip
        {
            get { return quickInfoTip; }
        }

    }
}
