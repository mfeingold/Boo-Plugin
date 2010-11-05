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
        private readonly Mapper mapper;
        private string quickInfoTip;
        private readonly ReferenceExpression node;
        private MappedNode defintionNode;

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
            try
            {
                var entity = TypeSystemServices.GetEntity(node);
                var prefix = "";
                if (entity is InternalParameter)
                {
                    prefix = "parameter";
                    defintionNode = mapper.GetNode(((InternalParameter)entity).Parameter.LexicalInfo);
                }
                if (entity is InternalLocal)
                {
                    prefix = "local";
                    defintionNode = mapper.GetNode(((InternalLocal)entity).Local.LexicalInfo);
                }
                if (entity is InternalField)
                {
                    prefix = "field";
                    defintionNode = mapper.GetNode(((InternalField)entity).Field.LexicalInfo);
                }
                quickInfoTip = "(" + prefix + ") " + node.Name + " as " + node.ExpressionType.FullName;

            }
            catch (Exception)
            {
                return;
            }
        }

        protected internal override MappedNode DefintionNode { get { return defintionNode; } }

    }
}
