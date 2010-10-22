using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Boo.Lang.Compiler.Ast;

namespace Hill30.BooProject.LanguageService
{
    public class TokenMapper
    {
        private int[][] positionMap;
        private readonly Dictionary<int, List<Tuple<Node,int,int>>> nodeDictionary = new Dictionary<int, List<Tuple<Node, int, int>>>();

        public TokenMapper(int tabSize, string source)
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
                else if (c=='\n')
                {
                    positionList.Add(sourcePos); // to map the <EOL> token
                    mappers.Add(positionList.ToArray());
                    positionList.Clear();
                    sourcePos = 0;
                    mappedPos = 0;
                }
                else
                {
                    positionList.Add(sourcePos++);
                    mappedPos++;
                }
            }
            positionList.Add(sourcePos); // to map the <EOL> token
            mappers.Add(positionList.ToArray());
            positionMap = mappers.ToArray();
        }

        public void MapNode(Node node, int length)
        {
            if (node.LexicalInfo != null)
            {
                List<Tuple<Node,int,int>> nodes;
                if (!this.nodeDictionary.TryGetValue(node.LexicalInfo.Line-1, out nodes))
                    this.nodeDictionary[node.LexicalInfo.Line-1] = nodes = new List<Tuple<Node, int, int>>();
                var start = positionMap[node.LexicalInfo.Line-1][node.LexicalInfo.Column - 1];
                var end = positionMap[node.LexicalInfo.Line-1][node.LexicalInfo.Column - 1 + length];
                nodes.Add(new Tuple<Node, int, int>(node, start, end));
            }
        }

        public Node GetNode(int line, int pos)
        {
            List<Tuple<Node, int, int>> nodes;
            if (!this.nodeDictionary.TryGetValue(line, out nodes))
                return null;
            foreach (var node in nodes)
            {
                if (pos >= node.Item2 && pos < node.Item3)
                    return node.Item1;
            }
            return null;
        }
    }
}
