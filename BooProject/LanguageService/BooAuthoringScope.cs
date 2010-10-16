using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.BooProject.LanguageService
{
    public class BooAuthoringScope : AuthoringScope
    {
        private readonly BooParseRequest req;

        public BooAuthoringScope(BooParseRequest req)
        {
            this.req = req;
        }

        public override string GetDataTipText(int line, int col, out TextSpan span)
        {
            return req.Source.GetDataTipText(line, col, out span);
        }

        public override Declarations GetDeclarations(IVsTextView view, int line, int col, TokenInfo info, ParseReason reason)
        {
            return null;
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
