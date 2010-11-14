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

using Boo.Lang.Compiler.Ast;
using Hill30.BooProject.LanguageService.Mapping.Nodes;

namespace Hill30.BooProject.LanguageService.Mapping
{
    public class FullAstWalker : DepthFirstVisitor
    {
        private readonly NodeMap nodeMap;
        private readonly BufferMap bufferMap;

        public FullAstWalker(NodeMap nodeMap, BufferMap bufferMap)
        {
            this.nodeMap = nodeMap;
            this.bufferMap = bufferMap;
        }

        public override void OnMemberReferenceExpression(MemberReferenceExpression node)
        {
            if (node.LexicalInfo != null)
                nodeMap.MapNode(new MappedReferenceExpression(nodeMap, bufferMap, node));
            base.OnMemberReferenceExpression(node);
        }

        protected override void OnError(Node node, System.Exception error)
        {
            //base.OnError(node, error);
        }
    }
}
