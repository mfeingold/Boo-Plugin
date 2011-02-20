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
using Boo.Lang.Compiler.TypeSystem;

namespace Hill30.Boo.ASTMapper.AST.Nodes
{
    public class MappedAttribute : MappedNode
    {
        private string quickInfoTip;
        private string format;

        public MappedAttribute(CompileResults results, Attribute node)
            : base(results, node, node.Name.Length)
        {
        }

        public override string Format
        {
            get { return format; }
        }

        public override string QuickInfoTip
        {
            get { return quickInfoTip; }
        }

        public override MappedNodeType Type
        {
            get { return MappedNodeType.TypeReference; }
        }

        protected override void ResolveImpl(MappedToken token)
        {
            try
            {
                var type = TypeSystemServices.GetType(Node);
                if (type is Error)
                    return;
//                quickInfoTip = "class " + type.FullName;
                format = Formats.BooType;
            }
            catch
            {
                return;
            }
        }
    }
}
