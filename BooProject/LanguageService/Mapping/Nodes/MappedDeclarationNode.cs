using Boo.Lang.Compiler.Ast;

namespace Hill30.BooProject.LanguageService.Mapping.Nodes
{
    public class MappedDeclarationNode : MappedNode
    {

        public MappedDeclarationNode(BufferMap bufferMap, ParameterDeclaration node)
            : base(bufferMap, node, 0)
        {
        }

        public MappedDeclarationNode(BufferMap bufferMap, Local node)
            : base(bufferMap, node, 0)
        {
        }

        public MappedDeclarationNode(BufferMap bufferMap, Field node)
            : base(bufferMap, node, 0)
        {
        }
    }
}
