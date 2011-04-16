using Boo.Lang.Compiler.Ast;
using Hill30.Boo.ASTMapper;
using Microsoft.VisualStudio.TextManager.Interop;
using NUnit.Framework;

namespace MapperTests
{
    [TestFixture]
    public class TypeReferenceTests : TestBase
    {
        [Test]
        public void IntTypeReference()
        {
            RunTest(
                new TestData
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
            RunTest(
                new TestData
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
            RunTest(
                new TestData
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
            RunTest(
                new TestData
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
            RunTest(
                new TestData
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
            RunTest(
                new TestData
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
            RunTest(
                new TestData
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
            RunTest(
                new TestData
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
            RunTest(
                new TestData
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
            RunTest(
                new TestData
                {
                    source = "class aClass\na as aClass",
                    location = new SourceLocation(2, 6),
                    expectedDatatip = "class aClass",
                    expectedFormat = Formats.BooType,
                    expectedSpan = new TextSpan { iStartLine = 1, iEndLine = 1, iStartIndex = 5, iEndIndex = 11 },
                    expectedDefinition = new TextSpan { iStartLine = 0, iEndLine = 1, iStartIndex = 6, iEndIndex = 11 }
                }
                );
        }

        [Test]
        public void CastTypeReference()
        {
            RunTest(
                new TestData
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
            RunTest(
                new TestData
                {
                    source = "o as object\ni=(o as int)",
                    location = new SourceLocation(2, 9),
                    expectedDatatip = "struct int",
                    expectedFormat = Formats.BooType,
                    expectedSpan = new TextSpan { iStartLine = 1, iEndLine = 1, iStartIndex = 8, iEndIndex = 11 }
                }
                );
        }

        [Test]
        public void AttributeShort()
        {
            RunTest(
                new TestData
                {
                    source = "[System.Serializable]\nclass c:\n\tdef constructor():\n\t\tpass",
                    location = new SourceLocation(1, 8),
                    expectedDatatip = "class System.SerializableAttribute",
                    expectedFormat = Formats.BooType,
                    // TODO: it should actually hihghlight only the class name - excluding the namespaces
                    expectedSpan = new TextSpan { iStartLine = 0, iEndLine = 0, iStartIndex = 1, iEndIndex = 20 }
                }
                );
        }

        [Test]
        public void AttributeFull()
        {
            RunTest(
                new TestData
                {
                    source = "[System.SerializableAttribute]\nclass c:\n\tdef constructor():\n\t\tpass",
                    location = new SourceLocation(1, 8),
                    expectedDatatip = "class System.SerializableAttribute",
                    expectedFormat = Formats.BooType,
                    // TODO: it should actually hihghlight only the class name - excluding the namespaces
                    expectedSpan = new TextSpan { iStartLine = 0, iEndLine = 0, iStartIndex = 1, iEndIndex = 29 }
                }
                );
        }

    }
}
