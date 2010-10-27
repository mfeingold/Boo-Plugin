using System;
using Microsoft.VisualStudio.Package;
using Boo.Lang.Parser;
using System.IO;
using Microsoft.VisualStudio.TextManager.Interop;

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
            //if (IsBlockComment(token))
            //{
            //    tokenInfo.Type = TokenType.Text;  // it has to be Text rather than Comment, otherwise there will be no notification for the typing inside the token
            //    tokenInfo.Color = TokenColor.Comment;
            //}
            //else
                switch (token.Type)
                {
                    case BooLexer.TRIPLE_QUOTED_STRING:
                        quotes = 6;
                        tokenInfo.Type = TokenType.String;
                        tokenInfo.Color = TokenColor.String;
                        break;

                    case BooLexer.DOUBLE_QUOTED_STRING:
                    case BooLexer.SINGLE_QUOTED_STRING:
                        quotes = 2;
                        tokenInfo.Type = TokenType.String;
                        tokenInfo.Color = TokenColor.String;
                        break;

                    case BooLexer.DOT:
                        tokenInfo.Type = TokenType.Text;
                        tokenInfo.Color = TokenColor.Text;
                        tokenInfo.Trigger = TokenTriggers.MemberSelect;
                        break;

                    case BooLexer.WS:
                        tokenInfo.Type = TokenType.WhiteSpace;
                        tokenInfo.Color = TokenColor.Text;
                        break;

                    case BooLexer.ID:
                        tokenInfo.Type = TokenType.Identifier;
                        tokenInfo.Color = GetColorForID(token);
                        break;

                    case BooLexer.ABSTRACT:
                    case BooLexer.AS:
                    case BooLexer.BREAK:
                    case BooLexer.CLASS:
                    case BooLexer.CONSTRUCTOR:
                    case BooLexer.CONTINUE:
                    case BooLexer.DEF:
                    case BooLexer.DO:
                    case BooLexer.ELIF:
                    case BooLexer.ELSE:
                    case BooLexer.ENUM:
                    case BooLexer.EVENT:
                    case BooLexer.EXCEPT:
                    case BooLexer.FALSE:
                    case BooLexer.FINAL:
                    case BooLexer.FOR:
                    case BooLexer.FROM:
                    case BooLexer.GET:
                    case BooLexer.GOTO:
                    case BooLexer.IF:
                    case BooLexer.IMPORT:
                    case BooLexer.IN:
                    case BooLexer.INTERFACE:
                    case BooLexer.INTERNAL:
                    case BooLexer.IS:
                    case BooLexer.LONG:
                    case BooLexer.NAMESPACE:
                    case BooLexer.NULL:
                    case BooLexer.OF:
                    case BooLexer.OVERRIDE:
                    case BooLexer.PARTIAL:
                    case BooLexer.PASS:
                    case BooLexer.PRIVATE:
                    case BooLexer.PROTECTED:
                    case BooLexer.PUBLIC:
                    case BooLexer.RAISE:
                    case BooLexer.REF:
                    case BooLexer.RETURN:
                    case BooLexer.SELF:
                    case BooLexer.SET:
                    case BooLexer.STATIC:
                    case BooLexer.STRUCT:
                    case BooLexer.SUPER:
                    case BooLexer.THEN:
                    case BooLexer.TRUE:
                    case BooLexer.TRY:
                    case BooLexer.TYPEOF:
                    case BooLexer.UNLESS:
                    case BooLexer.VIRTUAL:
                    case BooLexer.WHILE:
                    case BooLexer.YIELD:

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

        int lineNumber = -1;
        internal void SetLineNumber(int line)
        {
            lineNumber = line;
            if (source == null)
                source = service.GetSource(buffer);
        }

        private TokenColor GetColorForID(antlr.IToken token)
        {
            if (source == null)
                return TokenColor.Identifier;
            return ((BooSource)source).GetColorForID(lineNumber + 1, token);
        }

        private bool IsBlockComment(antlr.IToken token)
        {
            if (source == null)
                return false;
            return ((BooSource)source).IsBlockComment(lineNumber+1, token);
        }

    }
}
