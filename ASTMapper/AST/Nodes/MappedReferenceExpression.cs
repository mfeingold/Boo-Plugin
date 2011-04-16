//
//   Copyright © 2010 Michael Feingold
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System.Linq;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.TypeSystem;
using Boo.Lang.Compiler.TypeSystem.Internal;
using Boo.Lang.Compiler.TypeSystem.Reflection;

namespace Hill30.Boo.ASTMapper.AST.Nodes
{
    public class MappedReferenceExpression : MappedNode
    {
        private string quickInfoTip;
        private MappedNode declarationNode;
        private IType declaringType;
        private IType varType;
        private string format;
        private bool isTypeReference;

        public MappedReferenceExpression(CompileResults results, ReferenceExpression node)
            : base(results, node, node.Name.Length)
        {
        }

        public MappedReferenceExpression(CompileResults results, SelfLiteralExpression node)
            : base(results, node, "self".Length)
        {
        }

        public override MappedNodeType Type
        {
            get { return MappedNodeType.VraiableReference; }
        }

        public override string QuickInfoTip
        {
            get { return quickInfoTip; }
        }

        protected override void ResolveImpl(MappedToken token)
        {
            switch (Node.NodeType)
            {
                case NodeType.SelfLiteralExpression:
                    var classDefinition = Node;
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
                    var expression = (ReferenceExpression)Node;
                    IEntity entity;
                    try
                    {
                        entity = TypeSystemServices.GetEntity(expression);
                    }
                    catch
                    {
                        break;
                    }
                    var prefix = "";
                    if (entity is InternalParameter)
                    {
                        prefix = "(parameter) ";
                        varType = TypeSystemServices.GetType(expression);
                        declarationNode = CompileResults.GetMappedNode(((InternalParameter)entity).Parameter);
                    }
                    if (entity is InternalLocal)
                    {
                        prefix = "(local variable) ";
                        varType = ((InternalLocal)entity).Type;
                        declarationNode = CompileResults.GetMappedNode(((InternalLocal)entity).Local);
                    }
                    if (entity is InternalField)
                    {
                        varType = TypeSystemServices.GetType(Node);
                        declaringType = ((InternalField) entity).DeclaringType; 
                        declarationNode = CompileResults.GetMappedNode(((InternalField)entity).Field);
                    }
                    if (entity is InternalMethod)
                    {
                        declaringType = ((InternalMethod)entity).DeclaringType;
                        declarationNode = CompileResults.GetMappedNode(((InternalMethod)entity).Method);
                        if (entity is InternalConstructor)
                            varType = ((InternalConstructor) entity).DeclaringType;
                        else
                            varType = ((InternalMethod)entity).ReturnType;
                    }
                    if (entity is InternalProperty)
                    {
                        declaringType = ((InternalProperty)entity).DeclaringType;
                        varType = TypeSystemServices.GetType(Node);
                        declarationNode = CompileResults.GetMappedNode(((InternalProperty)entity).Property);
                    }
                    if (entity is InternalEvent)
                    {
                        declaringType = ((InternalEvent)entity).DeclaringType;
                        varType = TypeSystemServices.GetType(Node);
                        declarationNode = CompileResults.GetMappedNode(((InternalEvent)entity).Event);
                    }
                    if (entity is ExternalType)
                    {
                        varType = ((ExternalType)entity).Type;
                        format = Formats.BooType;
                        isTypeReference = true;
                    }
                    if (entity is AbstractInternalType)
                    {
                        varType = ((AbstractInternalType)entity).Type;
                        format = Formats.BooType;
                        isTypeReference = true;
                        declarationNode = CompileResults.GetMappedNode(((AbstractInternalType)entity).TypeDefinition);
                    }
                    if (entity is ExternalField)
                    {
                        varType = TypeSystemServices.GetType(Node);
                        declaringType = ((ExternalField)entity).DeclaringType;
//                        declarationNode = CompileResults.GetMappedNode(((ExternalField)entity).Field);
                    }
                    if (entity is ExternalMethod)
                    {
                        declaringType = ((ExternalMethod)entity).DeclaringType;
//                        declarationNode = CompileResults.GetMappedNode(declaration);
                        if (entity is ExternalConstructor)
                            varType = ((ExternalConstructor)entity).DeclaringType;
                        else
                            varType = ((ExternalMethod)entity).ReturnType;
                    }
                    if (entity is ExternalProperty)
                    {
                        declaringType = ((ExternalProperty)entity).DeclaringType;
                        varType = TypeSystemServices.GetType(Node);
//                        declarationNode = CompileResults.GetMappedNode(((ExternalProperty)entity).Property);
                    }
                    if (entity is ExternalEvent)
                    {
                        declaringType = ((ExternalEvent)entity).DeclaringType;
                        varType = TypeSystemServices.GetType(Node);
//                        declarationNode = CompileResults.GetMappedNode(((ExternalEvent)entity).Event);
                    }
                    if (expression.ExpressionType != null)
                    {
                        if (declaringType != null)
                            prefix += declaringType.FullName + '.';
                        quickInfoTip = prefix + expression.Name + " as " + expression.ExpressionType.FullName;
                    }
                    break;
                default:
                    break;
            }
        }

        public override string Format { get { return format; } }

        protected internal override MappedNode DeclarationNode { get { return declarationNode; } }

        private BooDeclarations declarations;
        
        public override BooDeclarations Declarations
        {
            get { return declarations ?? (declarations = new BooDeclarations(Node, varType, !isTypeReference)); }
        }

        internal override void Record(RecordingStage stage, MappedToken token)
        {
            switch (stage)
            {
                case RecordingStage.Completed:
                    var oldref = token.Nodes.Where(
                        node => (node.Node.NodeType == NodeType.ReferenceExpression &&
                                    ((ReferenceExpression)node.Node).Name == ((ReferenceExpression)Node).Name)
                        ).FirstOrDefault();
                    if (oldref != null)
                    {
                        token.Nodes.Remove(oldref);
                        base.Record(stage, token);
                    }
                    break;
                default:
                    base.Record(stage, token);
                    break;
            }
        }
    }
}
