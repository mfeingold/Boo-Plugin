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
        private readonly Node node;
        private MappedNode defintionNode;
        private IType varType;

        public MappedReferenceNode(Mapper mapper, ReferenceExpression node)
            : base(mapper, node, node.Name.Length)
        {
            this.mapper = mapper;
            this.node = node;
        }

        public MappedReferenceNode(Mapper mapper, SelfLiteralExpression node)
            : base(mapper, node, "self".Length)
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
            switch (node.NodeType)
            {
                case NodeType.SelfLiteralExpression:
                    var classDefinition = node;
                    while (classDefinition.ParentNode != null)
                        if (classDefinition.NodeType != NodeType.ClassDefinition)
                            classDefinition = classDefinition.ParentNode;
                        else
                        {
                            varType = TypeSystemServices.GetType(classDefinition);
                            break;
                        }
                    break;

                case NodeType.ReferenceExpression:
                    var expression = node as ReferenceExpression;
                    var entity = TypeSystemServices.GetEntity(expression);
                    var prefix = "";
                    if (entity is InternalParameter)
                    {
                        prefix = "(parameter) ";
                        varType = TypeSystemServices.GetType(expression);
                        defintionNode = mapper.GetNode(((InternalParameter)entity).Parameter.LexicalInfo);
                    }
                    if (entity is InternalLocal)
                    {
                        prefix = "(local variable) ";
                        varType = ((InternalLocal)entity).Type;
                        defintionNode = mapper.GetNode(((InternalLocal)entity).Local.LexicalInfo);
                    }
                    if (entity is InternalField)
                    {
                        varType = TypeSystemServices.GetType(node);
                        defintionNode = mapper.GetNode(((InternalField)entity).Field.LexicalInfo);
                    }
                    quickInfoTip = prefix + expression.Name + " as " + expression.ExpressionType.FullName;
                    break;
                default:
                    break;
            }
        }

        protected internal override MappedNode DefintionNode { get { return defintionNode; } }

        public override BooDeclarations Declarations
        {
            get
            {
                return new BooDeclarations(node, varType);
            }
        }
    }
}
