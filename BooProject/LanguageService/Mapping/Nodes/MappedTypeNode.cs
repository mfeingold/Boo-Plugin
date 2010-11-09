using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.TypeSystem;
using Hill30.BooProject.LanguageService.Colorizer;
using Boo.Lang.Compiler.TypeSystem.Reflection;

namespace Hill30.BooProject.LanguageService.Mapping.Nodes
{
    public class MappedTypeNode : MappedNode
    {
        private string format;
        private string quickInfoTip;

        public MappedTypeNode(NodeMap nodeMap, BufferMap bufferMap, ClassDefinition node)
            : base(bufferMap, node, node.Name.Length)
        {
            nodeMap.MapType(node);
            format = Formats.BooType;
        }

        public MappedTypeNode(BufferMap bufferMap, SimpleTypeReference node)
            : base(bufferMap, node, node.Name.Length)
        {
        }

        public MappedTypeNode(NodeMap nodeMap, BufferMap bufferMap, Module node)
            : base(bufferMap, node, node.Name.Length)
        {
            nodeMap.MapType(node);
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
            if (Node.NodeType == NodeType.SimpleTypeReference)
            {
                var type = TypeSystemServices.GetType(Node);
                if (type != null)
                {
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
            }
        }

    }
}
