using System;
using System.Collections.Generic;
using Boo.Lang.Parser;
using System.IO;

namespace Hill30.BooProject.LanguageService.Scanner
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

        internal MappedToken nextToken()
        {
            return new MappedToken(this, lexer.nextToken());
        }

        internal class MappedToken : antlr.IToken
        {
            private antlr.IToken iToken;
            private ScanningLexer lexer;

            public MappedToken(ScanningLexer scanningLexer, antlr.IToken iToken)
            {
                this.lexer = scanningLexer;
                this.iToken = iToken;
            }

            public int getMappedColumn()
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
