using System.Linq;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Boo.Lang.Compiler.Ast;

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
                var nodes = source.Mapper.GetNodes(line, col, node=>node.QuickInfoTip != null);
                int? start = null;
                int? end = null;
                var tip = "";
                foreach (var node in nodes)
                {
                    if (node.StartPos >= (start ?? 0))
                        start = node.StartPos;
                    if (node.EndPos <= (end ?? int.MaxValue))
                        end = node.EndPos;
                    if (tip != "")
                        tip += "\n";
                    tip += node.QuickInfoTip;
                }
                if (tip != "")
                {
                    span = new TextSpan
                               {iStartLine = line, iStartIndex = start.Value, iEndLine = line, iEndIndex = end.Value};
                    return tip;

                }
            }
            span = new TextSpan();
            return "";
        }

        public override Declarations GetDeclarations(IVsTextView view, int line, int col, TokenInfo info, ParseReason reason)
        {
            var node = source.Mapper.GetAdjacentNodes(line, col, n=>n.Declarations.GetCount() > 0).FirstOrDefault();
            if (node == null)
                return new BooDeclarations();
            return node.Declarations;
        }

        public override Methods GetMethods(int line, int col, string name)
        {
            return null;
        }

        public override string Goto(Microsoft.VisualStudio.VSConstants.VSStd97CmdID cmd, IVsTextView textView, int line, int col, out TextSpan span)
        {
            if (source.Mapper != null)
            {
                var node = source.Mapper.GetNodes(line, col, n=>n.DefintionNode != null).FirstOrDefault();
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
