using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boo.Lang.Compiler.Ast;
using Hill30.BooProject.LanguageService.Colorizer;

namespace Hill30.BooProject.LanguageService.NodeMapping
{
    public class MappedMacroNode: MappedNode
    {

        private string format;
        private string quickInfoTip;

        public MappedMacroNode(Mapper mapper, MacroStatement node)
            : base(mapper, node, node.Name.Length)
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
