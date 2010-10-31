using System;
using System.Collections.Generic;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.TypeSystem;
using Hill30.BooProject.LanguageService.Colorizer;
using Boo.Lang.Compiler.TypeSystem.Reflection;

namespace Hill30.BooProject.LanguageService.NodeMapping
{
    public class MappedTypeNode : MappedNode
    {
        private readonly string format;
        private readonly string quickInfoTip;

        public MappedTypeNode(Mapper mapper, ClassDefinition node)
            : base(mapper, node.LexicalInfo.Line, node.LexicalInfo.Column, node.Name.Length)
        {
            format = Formats.BooType;
            quickInfoTip = null;
        }

        public MappedTypeNode(Mapper mapper, SimpleTypeReference node)
            : base(mapper, node.LexicalInfo.Line, node.LexicalInfo.Column, node.Name.Length)
        {
            var type = TypeSystemServices.GetType(node);
            if (type != null)
            {
                format = Formats.BooType;
                var CLRType = type as ExternalType;
                if (CLRType != null)
                {
                    var prefix = "struct ";
                    if (CLRType.ActualType.IsClass)
                        prefix = "class ";
                    if (CLRType.ActualType.IsInterface)
                        prefix = "interface ";
                    if (CLRType.ActualType.IsEnum)
                        prefix = "enumeration ";
                    quickInfoTip = prefix + CLRType.ActualType.FullName;
                }
                else
                    quickInfoTip = "class " + type.FullName;
            }
        }

        public override string Format
        {
            get { return format; }
        }

        public override string QuickInfoTip
        {
            get { return quickInfoTip; }
        }

        public override IEnumerable<Tuple<string, string, int, string>> Declarations
        {
            get
            {
                var result = new List<Tuple<string, string, int, string>>();
                return result;
            }
        }

    }
}
