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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.TypeSystem;
using Hill30.BooProject.LanguageService.Colorizer;
using Boo.Lang.Compiler.TypeSystem.Reflection;

namespace Hill30.BooProject.AST.Nodes
{
    public class MappedTypeReference : MappedNode
    {
        private string format;
        private string quickInfoTip;

        public MappedTypeReference(CompileResults results, SimpleTypeReference node)
            : base(results, node, node.Name.Length)
        {
        }

        public override MappedNodeType Type
        {
            get { return MappedNodeType.TypeReference; }
        }

        public override string Format
        {
            get { return format; }
        }

        public override string QuickInfoTip
        {
            get { return quickInfoTip; }
        }

        protected override void ResolveImpl()
        {
            try
            {
                var type = TypeSystemServices.GetType(Node);
                if (type is Error)
                    return;

                format = Formats.BooType;
                var clrType = type as ExternalType;
                if (clrType != null)
                {
                    var prefix = "struct ";
                    if (clrType.ActualType.IsClass)
                        prefix = "class ";
                    if (clrType.ActualType.IsInterface)
                        prefix = "interface ";
                    if (clrType.ActualType.IsEnum)
                        prefix = "enumeration ";
                    quickInfoTip = prefix + clrType.ActualType.FullName;
                }
                else
                    quickInfoTip = "class " + type.FullName;
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
