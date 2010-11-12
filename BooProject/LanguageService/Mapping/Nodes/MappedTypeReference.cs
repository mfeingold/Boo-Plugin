using System;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.TypeSystem;
using Boo.Lang.Compiler.TypeSystem.Reflection;
using Hill30.BooProject.LanguageService.Colorizer;

namespace Hill30.BooProject.LanguageService.Mapping.Nodes
{
    public class MappedTypeReference : MappedNode
    {
        private string format;
        private string quickInfoTip;
        private readonly TypeReference node;

        public MappedTypeReference(BufferMap bufferMap, SimpleTypeReference node)
            : base(bufferMap, node.LexicalInfo, node.Name.Length)
        {
            this.node = node;
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

        internal protected override void Resolve()
        {
            try
            {
                var type = TypeSystemServices.GetType(node);
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
