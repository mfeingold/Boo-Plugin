using Boo.Lang.Compiler.Ast;
using Hill30.BooProject.LanguageService.Colorizer;

namespace Hill30.BooProject.LanguageService.Mapping.Nodes
{
    public class MappedMacroReference: MappedNode
    {

        private readonly string format;
        private readonly string quickInfoTip;

        public MappedMacroReference(BufferMap bufferMap, MacroStatement node)
            : base(bufferMap, node.LexicalInfo, node.Name.Length)
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
