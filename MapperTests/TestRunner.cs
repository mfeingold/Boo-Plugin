using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Hill30.BooProject.Compilation;
using Hill30.Boo.ASTMapper;
using System.Reflection;

namespace MapperTests
{
    [TestFixture]
    public class TestRunner
    {
        [Test, TestCaseSource("EnumerateTests")]
        public void Test(MapperTest test)
        {
            test.Run();
        }

        public IEnumerable<MapperTest> EnumerateTests()
        {
            return Tests.MapperTests;
        }
    }


    public class Source
    {
        public string Url { get; private set; }
        public string Source { get; private set; }
        public int TabSize { get; private set; }
        public Source(string url, string source, int tabSize)
        {
            Url = url;
            Source = source;
            TabSize = tabSize;
        }
    }

    public class MapperTest
    {
        CompileResults results;
        public MapperTest(Source source)
        {
            results = new CompileResults(() => source.Url, r => source.Source);
        }

        internal CompileResults Run()
        {
            CompilerManager.Compile(new Assembly[] { }, new CompileResults[] { results });
            return results;
        }
    }
}
