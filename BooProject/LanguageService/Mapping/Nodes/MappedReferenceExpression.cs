using System.Linq;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.TypeSystem;

namespace Hill30.BooProject.LanguageService.Mapping.Nodes
{
    public class MappedReferenceExpression : MappedNode
    {
        private readonly NodeMap nodeMap;
        private string quickInfoTip;
        private readonly Node node;
        private MappedNode declarationNode;
        private IType varType;

        public MappedReferenceExpression(NodeMap nodeMap, BufferMap bufferMap, ReferenceExpression node)
            : base(bufferMap, node.LexicalInfo, node.Name.Length)
        {
            this.nodeMap = nodeMap;
            this.node = node;
        }

        public MappedReferenceExpression(NodeMap nodeMap, BufferMap bufferMap, SelfLiteralExpression node)
            : base(bufferMap, node.LexicalInfo, "self".Length)
        {
            this.nodeMap = nodeMap;
            this.node = node;
        }

        public override MappedNodeType Type
        {
            get { return MappedNodeType.VraiableReference; }
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

                case NodeType.MemberReferenceExpression:
                case NodeType.ReferenceExpression:
                    var expression = (ReferenceExpression)node;
                    if (expression.ExpressionType == null || expression.ExpressionType.EntityType == EntityType.Error)
                        break;
                    var entity = TypeSystemServices.GetEntity(expression);
                    var prefix = "";
                    if (entity is InternalParameter)
                    {
                        prefix = "(parameter) ";
                        varType = TypeSystemServices.GetType(expression);
                        declarationNode = nodeMap.GetNodes(((InternalParameter)entity).Parameter.LexicalInfo, n=>n.Type == MappedNodeType.VariableDefinition).FirstOrDefault();
                    }
                    if (entity is InternalLocal)
                    {
                        prefix = "(local variable) ";
                        varType = ((InternalLocal)entity).Type;
                        declarationNode = nodeMap.GetNodes(((InternalLocal)entity).Local.LexicalInfo, n=>n.Type == MappedNodeType.VariableDefinition).FirstOrDefault();
                    }
                    if (entity is InternalField)
                    {
                        varType = TypeSystemServices.GetType(node);
                        declarationNode = nodeMap.GetNodes(((InternalField)entity).Field.LexicalInfo, n=>n.Type == MappedNodeType.VariableDefinition).FirstOrDefault();
                    }
                    quickInfoTip = prefix + expression.Name + " as " + expression.ExpressionType.FullName;
                    break;
                default:
                    break;
            }
        }

        protected internal override MappedNode DeclarationNode { get { return declarationNode; } }

        public override BooDeclarations Declarations
        {
            get
            {
                return new BooDeclarations(node, varType, true);
            }
        }
    }
}
