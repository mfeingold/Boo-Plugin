using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Package;
using Boo.Lang.Compiler;

namespace Hill30.BooProject.LanguageService
{
    public class BooSource : Source
    {
        public BooSource(Service service, Microsoft.VisualStudio.TextManager.Interop.IVsTextLines buffer, Colorizer colorizer)
            : base(service, buffer, colorizer)
        { }

        private CompilerContext compileResult;

        public CompilerContext CompileResult
        {
            get 
            {
                lock (this)
                {
                    return compileResult;
                }
            }
            set 
            {
                lock (this)
                {
                    compileResult = value;
                }
                BuildTokenDictionary();
            }
        }

        private void BuildTokenDictionary()
        {
//            compileResult.CompileUnit
        }

        internal bool IsBlockComment(antlr.IToken token)
        {
            return false;
        }
    }
}
