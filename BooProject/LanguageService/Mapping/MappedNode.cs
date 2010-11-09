using Boo.Lang.Compiler.Ast;

namespace Hill30.BooProject.LanguageService.Mapping
{
    public abstract class MappedNode
    {
        protected MappedNode(BufferMap bufferMap, Node node, int length)
        {
            Node = node;
            Length = length;
            Line = Node.LexicalInfo.Line;
            Column = Node.LexicalInfo.Column;
            StartPos = bufferMap.MapPosition(Node.LexicalInfo.Line, Node.LexicalInfo.Column);
            EndPos = bufferMap.MapPosition(Node.LexicalInfo.Line, Node.LexicalInfo.Column + length);
        }

        public Node Node { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }
        public int Length { get; private set; }
        public int StartPos { get; private set; }
        public int EndPos { get; private set; }
        public virtual string QuickInfoTip { get { return null; } }
        public virtual string Format { get { return null; } }
        public virtual BooDeclarations Declarations { get { return new BooDeclarations(); } }
        internal protected virtual void Resolve() { }
        internal protected virtual MappedNode DefintionNode { get { return null; } }

    }
}
