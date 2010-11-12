using System.Linq;
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
            var nodes = source.GetNodes(line, col, node=>node.QuickInfoTip != null);
            var tip = "";
            span = new TextSpan();
            foreach (var node in nodes)
            {
                span = span.Union(node.TextSpan);
                tip += node.QuickInfoTip;
            }
            return tip;
        }

        public override Declarations GetDeclarations(IVsTextView view, int line, int col, TokenInfo info, ParseReason reason)
        {
            var node = source.GetAdjacentNodes(line, col, n=>n.Declarations.GetCount() > 0).FirstOrDefault();
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
                var node = source.GetNodes(line, col, n=>n.DeclarationNode != null).FirstOrDefault();
            if (node != null)
            {
                if (node.DeclarationNode != null)
                {
                    span = node.DeclarationNode.TextSpan;
                    return node.DeclarationNode.LexicalInfo.FullPath;
                }
            }
            span = new TextSpan();
            return null;
        }
    }

}
