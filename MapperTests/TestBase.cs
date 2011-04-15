using System;
using System.IO;
using Boo.Lang.Compiler.Ast;
using Hill30.Boo.ASTMapper;
using Microsoft.VisualStudio.TextManager.Interop;
using NUnit.Framework;

namespace MapperTests
{
    public class TestBase
    {
        public static CompileResults RunCompiler(string source)
        {
            var results = new CompileResults(
                () => "Test",
                () => source,
                () => 4
                );
            CompilerManager.Compile(4, new[] { typeof(SerializableAttribute).Assembly }, new[] { results });
            return results;
        }

        // ReSharper disable InconsistentNaming
        public struct TestData
        {
            public string source;
            public SourceLocation location;
            public string expectedFormat;
            public string expectedDatatip;
            public TextSpan expectedSpan;
            public TextSpan? expectedDefinition;
        }
        // ReSharper restore InconsistentNaming

        public struct TestTextSpan
        {
            // ReSharper disable InconsistentNaming
            public TextSpan span;
            // ReSharper restore InconsistentNaming
            public override string ToString()
            {
                return string.Format("start=({0},{1}) end=({2},{3})", span.iStartLine, span.iStartIndex, span.iEndLine, span.iEndIndex);
            }
        }


        public static void RunTest(TestData testData)
        {
            var results = RunCompiler(testData.source);

            var mToken = results.GetMappedToken(testData.location);
            TextSpan ts;
            Assert.NotNull(mToken);
            Assert.Contains(testData.expectedFormat, mToken.Nodes.ConvertAll(n => n.Format));
            var tip = mToken.GetDataTiptext(out ts);
            Assert.AreEqual(testData.expectedDatatip, tip, "Expected '" + testData.expectedDatatip + "' but was '" + tip + "'");
            Assert.AreEqual(testData.expectedSpan, ts,
                "Expected Span" + (new TestTextSpan { span = testData.expectedSpan }) + " but was " + (new TestTextSpan { span = ts }));
            if (testData.expectedDefinition == null)
                return;
            var path = Directory.GetCurrentDirectory();
            Assert.AreEqual(Directory.GetCurrentDirectory() + "\\Test", mToken.Goto(out ts));
            Assert.AreEqual(testData.expectedDefinition.Value, ts,
                "Expected Definition " + (new TestTextSpan { span = testData.expectedDefinition.Value }) + " but was " + (new TestTextSpan { span = ts }));

        }

    }
}
