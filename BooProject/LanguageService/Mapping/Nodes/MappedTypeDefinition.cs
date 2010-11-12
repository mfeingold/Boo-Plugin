using Boo.Lang.Compiler.Ast;

namespace Hill30.BooProject.LanguageService.Mapping.Nodes
{
    public class MappedTypeDefinition : MappedNode
    {
        public TypeDefinition Node { get; private set; }

        public MappedTypeDefinition(BufferMap bufferMap, TypeDefinition node)
            : base(bufferMap, node)
        {
            Node = node;
        }

        public override MappedNodeType Type
        {
            get { return MappedNodeType.TypeDefiniton; }
        }

    }
}
