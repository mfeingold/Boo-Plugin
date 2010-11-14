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

using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace Hill30.BooProject.LanguageService.Mapping
{
    public class BufferMap
    {
        private int[][] positionMap;
        public ITextSnapshot CurrentSnapshot { get; private set; }
        public string FilePath { get; private set; }


        public void Map(ITextBuffer buffer, int tabSize)
        {
            CurrentSnapshot = buffer.CurrentSnapshot;
            var doc = buffer.Properties[typeof(ITextDocument)] as ITextDocument;
            if (doc != null)
                FilePath = doc.FilePath;

            var source = CurrentSnapshot.GetText();
            var sourcePos = 0;
            var mappedPos = 0;
            var mappers = new List<int[]>();
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
                if (c == '\n')
                {
                    mappers.Add(positionList.ToArray());
                    positionList.Clear();
                    sourcePos = 0;
                    mappedPos = 0;
                }
            }
            positionList.Add(sourcePos); // to map the <EOL> token
            mappers.Add(positionList.ToArray());
            positionMap = mappers.ToArray();
        }

        public struct BufferPoint
        {
            public int Line;
            public int Column;
        }

        public BufferPoint MapPosition(int line, int pos)
        {
            if (line == -1)
                line = positionMap.Length;
            if (pos == -1)
                pos = positionMap[line - 1].Length;
            return new BufferPoint {Line = line - 1, Column = positionMap[line - 1][pos - 1]};
        }
    }
}
