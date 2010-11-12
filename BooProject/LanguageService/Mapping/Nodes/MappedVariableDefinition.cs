using Boo.Lang.Compiler.Ast;

namespace Hill30.BooProject.LanguageService.Mapping.Nodes
{
    public class MappedVariableDefinition : MappedNode
    {

        public MappedVariableDefinition(BufferMap bufferMap, ParameterDeclaration node)
            : base(bufferMap, node)
        {
        }

        public MappedVariableDefinition(BufferMap bufferMap, Local node)
            : base(bufferMap, node)
        {
        }

        public MappedVariableDefinition(BufferMap bufferMap, Field node)
            : base(bufferMap, node)
        {
        }

        public override MappedNodeType Type
        {
            get { return MappedNodeType.VariableDefinition; }
        }

    }
}
