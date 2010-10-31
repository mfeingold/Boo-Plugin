
using System;
using System.Collections.Generic;

namespace Hill30.BooProject.LanguageService.NodeMapping
{
    public abstract class MappedNode
    {
        protected MappedNode(Mapper mapper, int lineNo, int pos, int length)
        {
            LineNo = lineNo;
            Pos = pos;
            Length = length;
            var location = mapper.MapLocation(lineNo, pos, length);
            StartPos = location.Item1;
            EndPos = location.Item2;
        }

        public int LineNo { get; private set; }
        public int Pos { get; private set; }
        public int Length { get; private set; }
        public int StartPos { get; private set; }
        public int EndPos { get; private set; }
        public abstract string QuickInfoTip { get; }
        public virtual string Format { get { return null; } }
        public virtual IEnumerable<Tuple<string, string, int, string>> Declarations { get { return new List<Tuple<string, string, int, string>>(); } }

    }
}
