//
//   Copyright © 2010 Michael Feingold
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using Boo.Lang.Parser;
using Microsoft.VisualStudio.Package;

namespace Hill30.Boo.ASTMapper.Scanner
{
    public class Scanner : IScanner
    {
        ScanningLexer lexer;
        ScanningLexer.MappedToken stashedToken;
        int offset;
        int current;
        int endIndex;
        readonly Func<int> getTabsize;

        public Scanner(Func<int> getTabsize)
        {
            this.getTabsize = getTabsize;
        }

        #region IScanner Members

        public bool ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state)
        {
            var token = stashedToken;
            stashedToken = null;
            if (token == null)
                try
                {
                    token = lexer.NextToken();
                }
                catch (Exception)
                {
                    tokenInfo.StartIndex = current;
                    tokenInfo.EndIndex = endIndex ;
                    tokenInfo.Type = TokenType.Text;  // it has to be Text rather than Comment, otherwise there will be no notification for the typing inside the token
                    tokenInfo.Color = TokenColor.Comment;
                    current = tokenInfo.EndIndex + 1;
                    stashedToken = null;
                    return true;
                }

            if (current < token.GetMappedColumn()-1)
            {
                tokenInfo.StartIndex = current;
                tokenInfo.EndIndex = Math.Min(endIndex, offset + token.GetMappedColumn() - 2);
                tokenInfo.Type = TokenType.Text;  // it has to be Text rather than Comment, otherwise there will be no notification for the typing inside the token
                tokenInfo.Color = TokenColor.Comment;
                current = tokenInfo.EndIndex + 2;
                stashedToken = token;
                return true;
            }

            if (token.Type == BooLexer.EOL || token.Type == BooLexer.EOF)
                return false;

            int quotes = 0;

            switch (GetTokenType(token))
            {
                case BooTokenType.DocumentString:
                    quotes = 6;
                    tokenInfo.Type = TokenType.String;
                    tokenInfo.Color = TokenColor.String;
                    break;

                case BooTokenType.String:
                    quotes = 2;
                    tokenInfo.Type = TokenType.String;
                    tokenInfo.Color = TokenColor.String;
                    break;

                case BooTokenType.MemberSelector:
                    tokenInfo.Type = TokenType.Text;
                    tokenInfo.Color = TokenColor.Text;
                    tokenInfo.Trigger = TokenTriggers.MemberSelect;
                    break;

                case BooTokenType.WhiteSpace:
                    tokenInfo.Type = TokenType.WhiteSpace;
                    tokenInfo.Color = TokenColor.Text;
                    break;

                case BooTokenType.Identifier:
                    tokenInfo.Type = TokenType.Identifier;
                    tokenInfo.Color = TokenColor.Text;
                    break;

                case BooTokenType.Keyword:
                    tokenInfo.Type = TokenType.Keyword;
                    tokenInfo.Color = TokenColor.Keyword;
                    break;

                default:
                    tokenInfo.Color = TokenColor.Text;
                    break;
            }

            tokenInfo.StartIndex = offset + token.GetMappedColumn() - 1;
            tokenInfo.EndIndex = Math.Min(endIndex, offset + quotes + token.GetMappedColumn() - 1 + token.getText().Length - 1);

            current = tokenInfo.EndIndex + 1;
            return true;
        }

// ReSharper disable ParameterHidesMember
        public void SetSource(string source, int offset)
// ReSharper restore ParameterHidesMember
        {
            endIndex = source.Length;
            current = this.offset = offset;
            lexer = new ScanningLexer(getTabsize(), source.Substring(offset));
        }

        #endregion

        public enum BooTokenType
        {
            DocumentString = 0,
            String = 1,
            MemberSelector = 2,
            WhiteSpace = 3,
            Identifier = 4,
            Keyword = 5,
            Other = 6,
        }

        public static BooTokenType GetTokenType(antlr.IToken token)
        {
            switch (token.Type)
            {
                case BooLexer.TRIPLE_QUOTED_STRING: return BooTokenType.DocumentString;

                case BooLexer.DOUBLE_QUOTED_STRING:
                case BooLexer.SINGLE_QUOTED_STRING:
                    return BooTokenType.String;

                case BooLexer.DOT: return BooTokenType.MemberSelector;

                case BooLexer.WS: return BooTokenType.WhiteSpace;

                case BooLexer.ID: return BooTokenType.Identifier;

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
                    return BooTokenType.Keyword;

                default: return BooTokenType.Other;
            }
        }
    }
}
