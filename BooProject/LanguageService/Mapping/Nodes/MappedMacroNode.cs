using Boo.Lang.Compiler.Ast;
using Hill30.BooProject.LanguageService.Colorizer;

namespace Hill30.BooProject.LanguageService.Mapping.Nodes
{
    public class MappedMacroNode: MappedNode
    {

        private string format;
        private string quickInfoTip;

        public MappedMacroNode(BufferMap bufferMap, MacroStatement node)
            : base(bufferMap, node, node.Name.Length)
        {
            format = Formats.BooMacro;
            quickInfoTip = "macro " + node.Name;
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
