using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.BooProject.LanguageService
{
    public class BooAuthoringScope : AuthoringScope
    {
        private readonly BooSource source;

        public BooAuthoringScope(BooSource source)
        {
            this.source = source;
        }

        public override string GetDataTipText(int line, int col, out TextSpan span)
        {
            if (source.Mapper != null)
            {
                var node = source.Mapper.GetNode(line, col);
                if (node != null)
                {
                    span = new TextSpan
                               {iStartLine = line, iStartIndex = node.StartPos, iEndLine = line, iEndIndex = node.EndPos};
                    return node.QuickInfoTip;

                }
            }
            span = new TextSpan();
            return "";
        }

        public override Declarations GetDeclarations(IVsTextView view, int line, int col, TokenInfo info, ParseReason reason)
        {
            var node = source.Mapper.GetAdjacentNode(line, col);
            if (node == null)
                return new BooDeclarations();
            return new BooDeclarations(node.Declarations);
        }

        public override Methods GetMethods(int line, int col, string name)
        {
            return null;
        }

        public override string Goto(Microsoft.VisualStudio.VSConstants.VSStd97CmdID cmd, IVsTextView textView, int line, int col, out TextSpan span)
        {
            if (source.Mapper != null)
            {
                var node = source.Mapper.GetNode(line, col);
                if (node != null)
                {
                    if (node.DefintionNode != null)
                    {
                        span = new TextSpan
                                   {
                                       iStartLine = node.DefintionNode.Line-1, 
                                       iStartIndex = node.DefintionNode.StartPos, 
                                       iEndLine = node.DefintionNode.Line-1, 
                                       iEndIndex = node.DefintionNode.EndPos
                                   };
                        return node.Node.LexicalInfo.FullPath;
                    }
                }
            }
            span = new TextSpan();
            return null;
        }
    }

}
