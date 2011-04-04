using Boo.Lang.Compiler.Pipelines;

namespace boocNET35
{
    public class PipeLine : CompileToFile
    {
        public PipeLine()
        {
            BreakOnErrors = false;
        }
    }
}
