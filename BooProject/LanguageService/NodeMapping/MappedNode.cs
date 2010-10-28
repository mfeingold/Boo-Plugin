
using System;
namespace Hill30.BooProject.LanguageService.NodeMapping
{
    public class MappedNode
    {
        public MappedNode(Mapper mapper, int lineNo, int pos, int length, string quickInfoTip)
        {
            LineNo = lineNo;
            Pos = pos;
            Length = length;
            QuickInfoTip = quickInfoTip;
            var location = mapper.MapLocation(lineNo, pos, length);
            Start = location.Item1;
            End = location.Item2;
        }

        public int LineNo { get; private set; }
        public int Pos { get; private set; }
        public int Length { get; private set; }
        public int Start { get; private set; }
        public int End { get; private set; }
        public string QuickInfoTip { get; private set; }
//        public virtual BooColorizer.TokenColor NodeColor { get { return BooColorizer.TokenColor.Identifier; } }

    }
}
