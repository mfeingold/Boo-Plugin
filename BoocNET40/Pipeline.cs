using Boo.Lang.Compiler.Pipelines;

namespace boocNET40
{
    public class PipeLine : CompileToFile
    {
        public PipeLine()
        {
            BreakOnErrors = false;
        }
    }
}
