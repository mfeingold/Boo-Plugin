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

using System;
using Boo.Lang.Compiler.Ast;
using Hill30.BooProject.LanguageService.Mapping.Nodes;

namespace Hill30.BooProject.LanguageService.Mapping
{
    public class ParsedAstWalker : DepthFirstVisitor
    {
        private readonly NodeMap nodeMap;
        private readonly BufferMap bufferMap;
 
        public ParsedAstWalker(NodeMap nodeMap, BufferMap bufferMap)
        {
            this.nodeMap = nodeMap;
            this.bufferMap = bufferMap;
        }

        public override void OnParameterDeclaration(ParameterDeclaration node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedVariableDefinition(bufferMap, node));
            base.OnParameterDeclaration(node);
        }

        public override void OnLocal(Local node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedVariableDefinition(bufferMap, node));
            base.OnLocal(node);
        }

        public override void OnField(Field node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedVariableDefinition(bufferMap, node));
            base.OnField(node);
        }

        public override void OnProperty(Property node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedTypeMemberDefinition(bufferMap, node));
            base.OnProperty(node);
        }

        public override void OnMethod(Method node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedTypeMemberDefinition(bufferMap, node));
            base.OnMethod(node);
        }

        public override void OnEvent(Event node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedTypeMemberDefinition(bufferMap, node));
            base.OnEvent(node);
        }

        public override void OnClassDefinition(ClassDefinition node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedTypeDefinition(bufferMap, node));
            base.OnClassDefinition(node);
        }

        public override void OnModule(Module node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedTypeDefinition(bufferMap, node));
            base.OnModule(node);
        }

        public override void OnSimpleTypeReference(SimpleTypeReference node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedTypeReference(bufferMap, node));
            base.OnSimpleTypeReference(node);
        }

        public override void OnReferenceExpression(ReferenceExpression node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedReferenceExpression(nodeMap, bufferMap, node));
            base.OnReferenceExpression(node);
        }

        public override void OnSelfLiteralExpression(SelfLiteralExpression node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedReferenceExpression(nodeMap, bufferMap, node));
            base.OnSelfLiteralExpression(node);
        }

        public override void OnMemberReferenceExpression(MemberReferenceExpression node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedReferenceExpression(nodeMap, bufferMap, node));
            base.OnMemberReferenceExpression(node);
        }

        public override void OnMacroStatement(MacroStatement node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedMacroReference(bufferMap, node));
            base.OnMacroStatement(node);
        }

        protected override void OnError(Node node, Exception error)
        {
            // Do Nothing
        }
    }

}
