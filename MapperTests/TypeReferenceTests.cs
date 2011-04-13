using System;
using Boo.Lang.Compiler.Ast;
using Hill30.Boo.ASTMapper;
using Microsoft.VisualStudio.TextManager.Interop;
using NUnit.Framework;

namespace MapperTests
{
    [TestFixture]
    public class TypeReferenceTests
    {
        private static CompileResults RunCompiler(string source)
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
        struct TypeReferenceTestData
        {
            public string source;
            public SourceLocation location;
            public string expectedFormat;
            public string expectedDatatip;
            public TextSpan expectedSpan;
        }
        // ReSharper restore InconsistentNaming

        struct TestTextSpan
        {
// ReSharper disable InconsistentNaming
            public TextSpan span;
// ReSharper restore InconsistentNaming
            public override string ToString()
            {
                return string.Format("start=({0},{1}) end=({2},{3})", span.iStartLine, span.iStartIndex, span.iEndLine, span.iEndIndex);
            }
        }


        private static void TypeReferenceTest(TypeReferenceTestData testData)
        {
            var results = RunCompiler(testData.source);

            var mToken = results.GetMappedToken(testData.location);
            TextSpan ts;
            Assert.NotNull(mToken);
            Assert.Contains(testData.expectedFormat, mToken.Nodes.ConvertAll(n=>n.Format));
            Assert.AreEqual(testData.expectedDatatip, mToken.GetDataTiptext(out ts));
            Assert.AreEqual(testData.expectedSpan, ts, 
                "Expected " + (new TestTextSpan{span = testData.expectedSpan}) + " but was " + (new TestTextSpan{span=ts}));
        }

        [Test]
        public void IntTypeReference()
        {
            TypeReferenceTest(
                new TypeReferenceTestData
                {
                    source = "a as int",
                    location = new SourceLocation(1, 6),
                    expectedDatatip = "struct int",
                    expectedFormat = Formats.BooType,
                    expectedSpan = new TextSpan { iStartLine = 0, iEndLine = 0, iStartIndex = 5, iEndIndex = 8 }
                }
                );
        }

        [Test]
        public void StringTypeReference()
        {
            TypeReferenceTest(
                new TypeReferenceTestData
                {
                    source = "a as string",
                    location = new SourceLocation(1, 6),
                    expectedDatatip = "class string",
                    expectedFormat = Formats.BooType,
                    expectedSpan = new TextSpan { iStartLine = 0, iEndLine = 0, iStartIndex = 5, iEndIndex = 11 }
                }
                );
        }

        [Test]
        public void BoolTypeReference()
        {
            TypeReferenceTest(
                new TypeReferenceTestData
                {
                    source = "a as bool",
                    location = new SourceLocation(1, 6),
                    expectedDatatip = "struct bool",
                    expectedFormat = Formats.BooType,
                    expectedSpan = new TextSpan { iStartLine = 0, iEndLine = 0, iStartIndex = 5, iEndIndex = 9 }
                }
                );
        }

        [Test]
        public void CharTypeReference()
        {
            TypeReferenceTest(
                new TypeReferenceTestData
                {
                    source = "a as char",
                    location = new SourceLocation(1, 6),
                    expectedDatatip = "struct char",
                    expectedFormat = Formats.BooType,
                    expectedSpan = new TextSpan { iStartLine = 0, iEndLine = 0, iStartIndex = 5, iEndIndex = 9 }
                }
                );
        }

        [Test]
        public void RegularTypeReference()
        {
            TypeReferenceTest(
                new TypeReferenceTestData
                {
                    source = "a as System.Collections.ArrayList",
                    location = new SourceLocation(1, 6),
                    expectedDatatip = "class System.Collections.ArrayList",
                    expectedFormat = Formats.BooType,
                    // TODO: it should actually hihghlight only the class name - excluding the namespaces
                    expectedSpan = new TextSpan { iStartLine = 0, iEndLine = 0, iStartIndex = 5, iEndIndex = 33 }
                }
                );
        }

        [Test]
        public void RegularTypeReferenceAbbreviated()
        {
            TypeReferenceTest(
                new TypeReferenceTestData
                {
                    source = "import System.Collections\na as ArrayList",
                    location = new SourceLocation(2, 6),
                    expectedDatatip = "class System.Collections.ArrayList",
                    expectedFormat = Formats.BooType,
                    expectedSpan = new TextSpan { iStartLine = 1, iEndLine = 1, iStartIndex = 5, iEndIndex = 14 }
                }
                );
        }

        [Test]
        public void GenericTypeReference()
        {
            TypeReferenceTest(
                new TypeReferenceTestData
                {
                    source = "import System.Collections.Generic\na as List[of int]",
                    location = new SourceLocation(2, 6),
                    expectedDatatip = "class System.Collections.Generic.List",
                    expectedFormat = Formats.BooType,
                    // TODO: it should actually hihghlight only the class name - excluding the namespaces
                    expectedSpan = new TextSpan { iStartLine = 1, iEndLine = 1, iStartIndex = 5, iEndIndex = 9 }
                }
                );
        }

        [Test]
        public void GenericTypeParameterReference()
        {
            TypeReferenceTest(
                new TypeReferenceTestData
                {
                    source = "import System.Collections.Generic\na as List[of int]",
                    location = new SourceLocation(2, 14),
                    expectedDatatip = "struct int",
                    expectedFormat = Formats.BooType,
                    expectedSpan = new TextSpan { iStartLine = 1, iEndLine = 1, iStartIndex = 13, iEndIndex = 16 }
                }
                );
        }

        [Test]
        public void BaseTypeReference()
        {
            TypeReferenceTest(
                new TypeReferenceTestData
                {
                    source = "class a(System.Exception)",
                    location = new SourceLocation(1, 10),
                    expectedDatatip = "class System.Exception",
                    expectedFormat = Formats.BooType,
                    expectedSpan = new TextSpan { iStartLine = 0, iEndLine = 0, iStartIndex = 8, iEndIndex = 24 }
                }
                );
        }

        [Test]
        public void LocalTypeReference()
        {
            TypeReferenceTest(
                new TypeReferenceTestData
                {
                    source = "class aClass\na as aClass",
                    location = new SourceLocation(2, 6),
                    expectedDatatip = "class aClass",
                    expectedFormat = Formats.BooType,
                    expectedSpan = new TextSpan { iStartLine = 1, iEndLine = 1, iStartIndex = 5, iEndIndex = 11 }
                }
                );
        }

        [Test]
        public void CastTypeReference()
        {
            TypeReferenceTest(
                new TypeReferenceTestData
                {
                    source = "i as int\no as object\ni=cast(int, o)",
                    location = new SourceLocation(3, 8),
                    expectedDatatip = "struct int",
                    expectedFormat = Formats.BooType,
                    expectedSpan = new TextSpan { iStartLine = 2, iEndLine = 2, iStartIndex = 7, iEndIndex = 10 }
                }
                );
        }

        [Test]
        public void AsTypeReference()
        {
            TypeReferenceTest(
                new TypeReferenceTestData
                {
                    source = "o as object\ni=(o as int)",
                    location = new SourceLocation(2, 9),
                    expectedDatatip = "struct int",
                    expectedFormat = Formats.BooType,
                    expectedSpan = new TextSpan { iStartLine = 1, iEndLine = 1, iStartIndex = 8, iEndIndex = 11 }
                }
                );
        }

    }
}
