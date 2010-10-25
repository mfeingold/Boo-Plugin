using Boo.Lang.Compiler.Ast;

namespace Hill30.BooProject.LanguageService.NodeMapping
{
    public class MappedTypeNode : MappedNode
    {

        public MappedTypeNode(Mapper mapper, ClassDefinition node)
            : base(mapper, node.LexicalInfo.Line, node.LexicalInfo.Column, node.Name.Length, null)
        {
        }

        public MappedTypeNode(Mapper mapper, SimpleTypeReference node)
            : base(mapper, node.LexicalInfo.Line, node.LexicalInfo.Column, node.Name.Length, "class " + node.Name)
        {
        }

        public override BooColorizer.TokenColor NodeColor { get { return BooColorizer.TokenColor.Type; } }
    }
}
