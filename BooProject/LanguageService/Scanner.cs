using System;
using Microsoft.VisualStudio.Package;
using Boo.Lang.Parser;
using System.IO;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Hill30.BooProject.LanguageService
{
    class Scanner : IScanner
    {
        antlr.TokenStream lexer;
        antlr.IToken stashedToken;
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

        class DummyToken : antlr.IToken
        {
            public int Type
            {
                get
                {
                    return BooLexer.EOF;
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public int getColumn()
            {
                return 0;
            }

            public string getFilename()
            {
                throw new NotImplementedException();
            }

            public int getLine()
            {
                throw new NotImplementedException();
            }

            public string getText()
            {
                throw new NotImplementedException();
            }

            public void setColumn(int c)
            {
                throw new NotImplementedException();
            }

            public void setFilename(string name)
            {
                throw new NotImplementedException();
            }

            public void setLine(int l)
            {
                throw new NotImplementedException();
            }

            public void setText(string t)
            {
                throw new NotImplementedException();
            }
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
                    tokenInfo.Type = TokenType.Text;  // it has to be Text rather than Comment, otherwise there will be no notofication for the typing inside the token
                    tokenInfo.Color = TokenColor.Comment;
                    current = tokenInfo.EndIndex + 1;
                    stashedToken = new DummyToken();
                    return true;
                }

            if (current < token.getColumn()-1)
            {
                tokenInfo.StartIndex = current;
                tokenInfo.EndIndex = Math.Min(endIndex, offset + token.getColumn() - 2);
                tokenInfo.Type = TokenType.Text;  // it has to be Text rather than Comment, otherwise there will be no notofication for the typing inside the token
                tokenInfo.Color = TokenColor.Comment;
                current = tokenInfo.EndIndex + 2;
                stashedToken = token;
                return true;
            }

            if (token.Type == BooLexer.EOL || token.Type == BooLexer.EOF)
                return false;

            int quotes = 0;
            if (IsBlockComment(token))
            {
                tokenInfo.Type = TokenType.Text;  // it has to be Text rather than Comment, otherwise there will be no notofication for the typing inside the token
                tokenInfo.Color = TokenColor.Comment;
            }
            else
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
                        tokenInfo.Color = TokenColor.Identifier;
                        break;

                    case BooLexer.ABSTRACT:
                    case BooLexer.AS:
                    case BooLexer.BREAK:
                    case BooLexer.CLASS:
                    case BooLexer.CONSTRUCTOR:
                    case BooLexer.CONTINUE:
                    case BooLexer.DEF:
                    case BooLexer.DO:
                    //case BooLexer.DOUBLE:
                    case BooLexer.ELIF:
                    case BooLexer.ELSE:
                    case BooLexer.ENUM:
                    case BooLexer.EVENT:
                    case BooLexer.EXCEPT:
                    case BooLexer.FALSE:
                    case BooLexer.FINAL:
                    //case BooLexer.FLOAT:
                    case BooLexer.FOR:
                    case BooLexer.FROM:
                    case BooLexer.GET:
                    case BooLexer.GOTO:
                    case BooLexer.IF:
                    case BooLexer.IMPORT:
                    case BooLexer.IN:
                    //case BooLexer.INT:
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

            tokenInfo.StartIndex = offset + token.getColumn() - 1;
            tokenInfo.EndIndex = Math.Min(endIndex, offset + quotes + token.getColumn() - 1 + token.getText().Length - 1);
            current = tokenInfo.EndIndex + 1;
            return true;
        }

// ReSharper disable ParameterHidesMember
        public void SetSource(string source, int offset)
// ReSharper restore ParameterHidesMember
        {
            endIndex = source.Length;
            current = this.offset = offset;
//            current = offset - 1;
            lexer = BooParser.CreateBooLexer(1, "Line Scanner", new StringReader(source.Substring(offset) + " "));
        }

        #endregion

        int lineNumber = -1;
        internal void SetLineNumber(int line)
        {
            lineNumber = line;
            if (source == null)
                source = service.GetSource(buffer);
        }

        private bool IsBlockComment(antlr.IToken token)
        {
            if (source == null)
                return false;
            return ((BooSource)source).IsBlockComment(lineNumber+1, token);
        }

    }
}
