
using System;
using System.Collections.Generic;
using Boo.Lang.Compiler.Ast;

namespace Hill30.BooProject.LanguageService.NodeMapping
{
    public abstract class MappedNode
    {
        protected MappedNode(Mapper mapper, Node node, int length)
        {
            Node = node;
            Length = length;
            Line = Node.LexicalInfo.Line;
            Column = Node.LexicalInfo.Column;
            var location = mapper.MapLocation(Node.LexicalInfo.Line, Node.LexicalInfo.Column, length);
            StartPos = location.Item1;
            EndPos = location.Item2;
        }

        public Node Node { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }
        public int Length { get; private set; }
        public int StartPos { get; private set; }
        public int EndPos { get; private set; }
        public abstract string QuickInfoTip { get; }
        public virtual string Format { get { return null; } }
        public virtual IEnumerable<Tuple<string, string, int, string>> Declarations { get { return new List<Tuple<string, string, int, string>>(); } }
        internal protected virtual void Resolve() { }
    }
}
