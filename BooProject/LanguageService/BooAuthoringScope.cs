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
            return source.GetDataTipText(line, col, out span);
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
            span = new TextSpan();
            return null;
        }
    }

}
