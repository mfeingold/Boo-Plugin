using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.BooProject.LanguageService
{
    class Parser : IScanner
    {
        private IVsTextLines buffer;
        private int state;

        public Parser(IVsTextLines buffer)
        {
            this.buffer = buffer;
        }

        string source;


        #region IScanner Members

        public bool ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state)
        {
         
            tokenInfo.Type = TokenType.Comment;
            tokenInfo.StartIndex = state;
            tokenInfo.EndIndex = state + 1;

            state = tokenInfo.EndIndex;
           
            return state < source.Length;
        }

        public void SetSource(string source, int offset)
        {
            this.source = source.Substring(offset);
        }

        #endregion
    }
}
