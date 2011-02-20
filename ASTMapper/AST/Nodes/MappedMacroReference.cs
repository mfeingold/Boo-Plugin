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

namespace Hill30.Boo.ASTMapper.AST.Nodes
{
    public class MappedMacroReference : MappedNode
    {

        private readonly string format;
        private readonly string quickInfoTip;

        public MappedMacroReference(CompileResults results, MacroStatement node)
            : base(results, node, node.Name.Length)
        {
            format = Formats.BooMacro;
            quickInfoTip = "macro " + node.Name;
        }

        public override MappedNodeType Type
        {
            get { return MappedNodeType.MacroReference; }
        }

        public override string QuickInfoTip
        {
            get { return quickInfoTip; }
        }

        public override string Format
        {
            get { return format; }
        }
    }
}
