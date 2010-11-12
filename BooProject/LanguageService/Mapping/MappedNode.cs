using Boo.Lang.Compiler.Ast;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.BooProject.LanguageService.Mapping
{
    public enum MappedNodeType
    {
        MacroReference,
        TypeReference,
        TypeDefiniton,
        VraiableReference,
        VariableDefinition,
    }

    public abstract class MappedNode
    {
        protected MappedNode(BufferMap bufferMap, LexicalInfo lexicalInfo, int length)
            : this(
                bufferMap.MapPosition(lexicalInfo.Line, lexicalInfo.Column),
                bufferMap.MapPosition(lexicalInfo.Line, lexicalInfo.Column + length)
            )
        {
            LexicalInfo = lexicalInfo;
        }

        protected MappedNode(BufferMap bufferMap, Node node)
            : this(
                bufferMap.MapPosition(node.LexicalInfo.Line, node.LexicalInfo.Column),
                bufferMap.MapPosition(node.EndSourceLocation.Line, node.EndSourceLocation.Column)
            )
        {
            LexicalInfo = node.LexicalInfo;
        }

        private MappedNode(BufferMap.BufferPoint start, BufferMap.BufferPoint end)
        {
            TextSpan = new TextSpan
            {
                iStartLine = start.Line,
                iStartIndex = start.Column,
                iEndLine = end.Line,
                iEndIndex = end.Column
            };
        }

        public LexicalInfo LexicalInfo { get; private set; }
        public TextSpan TextSpan { get; private set; }
        public virtual string QuickInfoTip { get { return null; } }
        public virtual string Format { get { return null; } }
        public virtual BooDeclarations Declarations { get { return new BooDeclarations(); } }
        internal protected virtual void Resolve() { }
        internal protected virtual MappedNode DeclarationNode { get { return null; } }
        public abstract MappedNodeType Type { get; }

    }
}
