using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boo.Lang.Compiler.Ast;

namespace Hill30.BooProject.LanguageService.NodeMapping
{
    public class Mapper
    {
        private readonly int[][] positionMap;
        private readonly Dictionary<int, List<MappedNode>> nodeDictionary = new Dictionary<int, List<MappedNode>>();

        public Mapper(int tabSize, string source)
        {
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
                if (c=='\n')
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

        public void MapNode(MappedNode node)
        {
            List<MappedNode> nodes;
            if (!nodeDictionary.TryGetValue(node.LineNo-1, out nodes))
                nodeDictionary[node.LineNo-1] = nodes = new List<MappedNode>();
            nodes.Add(node);
        }

        public MappedNode GetNode(int line, int pos)
        {
            List<MappedNode> nodes;
            if (!nodeDictionary.TryGetValue(line, out nodes))
                return null;
            foreach (var node in nodes)
            {
                if (pos >= node.Start && pos < node.End)
                    return node;
            }
            return null;
        }

        internal Tuple<int, int> MapLocation(int lineNo, int pos, int length)
        {
            return new Tuple<int, int>(
                positionMap[lineNo - 1][pos - 1],
                positionMap[lineNo - 1][pos - 1 + length]
                );
        }

        internal MappedNode GetNode(int line, antlr.IToken token)
        {
            List<MappedNode> nodes;
            if (!nodeDictionary.TryGetValue(line - 1, out nodes))
                return null;
            foreach (var node in nodes)
            {
                if (token.getColumn() == node.Pos && token.getText().Length == node.Length)
                    return node;
            }
            return null;
        }

    }
}
