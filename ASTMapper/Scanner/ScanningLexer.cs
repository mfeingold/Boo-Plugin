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
using System.Collections.Generic;
using System.IO;
using Boo.Lang.Parser;

namespace Hill30.Boo.ASTMapper.Scanner
{
    class ScanningLexer
    {
        readonly antlr.TokenStream lexer;
        readonly int[] positionMap;

        public ScanningLexer(int tabSize, string source)
        {
            source += " "; // No idea why but without this extra space the lexer sometimes throws an exception
            lexer = BooParser.CreateBooLexer(tabSize, "Line Scanner", new StringReader(source));
            var sourcePos = 0;
            var mappedPos = 0;
            var positionList = new List<int>();
            foreach (var c in source)
            {
                if (c == '\t')
                    while (mappedPos % tabSize < tabSize - 1)
                    {
                        positionList.Add(sourcePos);
                        mappedPos++;
                    }
                positionList.Add(sourcePos++);
                mappedPos++;
            }
            positionList.Add(sourcePos); // to map the <EOL> token
            positionMap = positionList.ToArray();
        }

        internal MappedToken NextToken()
        {
            return new MappedToken(this, lexer.nextToken());
        }

        internal class MappedToken : antlr.IToken
        {
            private readonly antlr.IToken iToken;
            private readonly ScanningLexer lexer;

            public MappedToken(ScanningLexer scanningLexer, antlr.IToken iToken)
            {
                lexer = scanningLexer;
                this.iToken = iToken;
            }

            public int GetMappedColumn()
            {
                return lexer.positionMap[iToken.getColumn()-1]+1;
            }

            #region IToken Members

            public int Type
            {
                get
                {
                    return iToken.Type;
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public int getColumn()
            {
                return iToken.getColumn();
            }

            public string getFilename()
            {
                return iToken.getFilename();
            }

            public int getLine()
            {
                return iToken.getLine();
            }

            public string getText()
            {
                return iToken.getText();
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

            #endregion
        }
    }
}
