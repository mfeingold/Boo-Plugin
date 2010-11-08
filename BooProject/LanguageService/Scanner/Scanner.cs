using System;
using Microsoft.VisualStudio.Package;
using Boo.Lang.Parser;
using System.IO;
using Microsoft.VisualStudio.TextManager.Interop;
using Hill30.BooProject.LanguageService.Colorizer;
using Hill30.BooProject.LanguageService.NodeMapping;

namespace Hill30.BooProject.LanguageService.Scanner
{
    class Scanner : IScanner
    {
        ScanningLexer lexer;
        ScanningLexer.MappedToken stashedToken;
        int offset;
        int current;
        int endIndex;
        private Service service;
        private IVsTextLines buffer;

        private Source source;

        public Scanner(Source source)
        {
            this.source = source;
        }

        public Scanner(Service service, IVsTextLines buffer)
        {
            this.service = service;
            this.buffer = buffer;
        }

        #region IScanner Members

        public bool ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state)
        {
            var token = stashedToken;
            stashedToken = null;
            if (token == null)
                try
                {
                    token = lexer.nextToken();
                }
                catch (Exception)
                {
                    tokenInfo.StartIndex = current;
                    tokenInfo.EndIndex = endIndex ;
                    tokenInfo.Type = TokenType.Text;  // it has to be Text rather than Comment, otherwise there will be no notification for the typing inside the token
                    tokenInfo.Color = TokenColor.Comment;
                    current = tokenInfo.EndIndex + 1;
                    stashedToken = null;//  new DummyToken();
                    return true;
                }

            if (current < token.getMappedColumn()-1)
            {
                tokenInfo.StartIndex = current;
                tokenInfo.EndIndex = Math.Min(endIndex, offset + token.getMappedColumn() - 2);
                tokenInfo.Type = TokenType.Text;  // it has to be Text rather than Comment, otherwise there will be no notification for the typing inside the token
                tokenInfo.Color = TokenColor.Comment;
                current = tokenInfo.EndIndex + 2;
                stashedToken = token;
                return true;
            }

            if (token.Type == BooLexer.EOL || token.Type == BooLexer.EOF)
                return false;

            int quotes = 0;

            switch (NodeMap.GetTokenType(token))
            {
                case NodeMap.TokenType.DocumentString:
                    quotes = 6;
                    tokenInfo.Type = TokenType.String;
                    tokenInfo.Color = TokenColor.String;
                    break;

                case NodeMap.TokenType.String:
                    quotes = 2;
                    tokenInfo.Type = TokenType.String;
                    tokenInfo.Color = TokenColor.String;
                    break;

                case NodeMap.TokenType.MemberSelector:
                    tokenInfo.Type = TokenType.Text;
                    tokenInfo.Color = TokenColor.Text;
                    tokenInfo.Trigger = TokenTriggers.MemberSelect;
                    break;

                case NodeMap.TokenType.WhiteSpace:
                    tokenInfo.Type = TokenType.WhiteSpace;
                    tokenInfo.Color = TokenColor.Text;
                    break;

                case NodeMap.TokenType.Identifier:
                    tokenInfo.Type = TokenType.Identifier;
                    tokenInfo.Color = TokenColor.Text;
                    break;

                case NodeMap.TokenType.Keyword:
                    tokenInfo.Type = TokenType.Keyword;
                    tokenInfo.Color = TokenColor.Keyword;
                    break;

                default:
                    tokenInfo.Color = TokenColor.Text;
                    break;
            }

            tokenInfo.StartIndex = offset + token.getMappedColumn() - 1;
            tokenInfo.EndIndex = Math.Min(endIndex, offset + quotes + token.getMappedColumn() - 1 + token.getText().Length - 1);

            current = tokenInfo.EndIndex + 1;
            return true;
        }

// ReSharper disable ParameterHidesMember
        public void SetSource(string source, int offset)
// ReSharper restore ParameterHidesMember
        {
            endIndex = source.Length;
            current = this.offset = offset;
            lexer = new ScanningLexer(service.GetLanguagePreferences().TabSize, source.Substring(offset));
        }

        #endregion

    }
}
