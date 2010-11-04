using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.TypeSystem.Services;
using Boo.Lang.Compiler.TypeSystem;

namespace Hill30.BooProject.LanguageService.NodeMapping
{
    public class MappedReferenceNode : MappedNode
    {
        private Mapper mapper;
        private string quickInfoTip;
        private ReferenceExpression node;
        private LexicalInfo gotoLocation;

        public MappedReferenceNode(Mapper mapper, ReferenceExpression node)
            : base(mapper, node, node.Name.Length)
        {
            this.mapper = mapper;
            this.node = node;
        }

        public override string QuickInfoTip
        {
            get { return quickInfoTip; }
        }

        protected internal override void Resolve()
        {
            if (node.ExpressionType is Error)
                return;
            var entity = TypeSystemServices.GetEntity(node);
            var prefix = "";
            if (entity is InternalParameter)
            {
                prefix = "parameter";
                gotoLocation = ((InternalParameter)entity).Parameter.LexicalInfo;
            }
            quickInfoTip = "(" + prefix + ") " + node.Name + " as " + node.ExpressionType.FullName;
        }

    }
}
