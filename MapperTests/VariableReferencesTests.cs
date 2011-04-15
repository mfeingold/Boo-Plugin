using Boo.Lang.Compiler.Ast;
using Microsoft.VisualStudio.TextManager.Interop;
using NUnit.Framework;

namespace MapperTests
{
    [TestFixture]
    class VariableReferencesTests : TestBase
    {

        [Test]
        public void ImplicitGlobalVariableInt()
        {
            RunTest(
                new TestData
                {
                    source = "a = 1",
                    location = new SourceLocation(1, 1),
                    // TODO: should it be 'global variable'?
                    expectedDatatip = "(local variable) a as int",
                    expectedSpan = new TextSpan { iStartLine = 0, iEndLine = 0, iStartIndex = 0, iEndIndex = 1 },
                    expectedDefinition = new TextSpan { iStartLine = 0, iEndLine = 0, iStartIndex = 0, iEndIndex = 1 }
                }
                );
        }

        [Test]
        public void ImplicitGlobalVariableChar()
        {
            RunTest(
                new TestData
                {
                    source = "a = char('a')",
                    location = new SourceLocation(1, 1),
                    // TODO: should it be 'global variable'?
                    expectedDatatip = "(local variable) a as char",
                    expectedSpan = new TextSpan { iStartLine = 0, iEndLine = 0, iStartIndex = 0, iEndIndex = 1 }
                }
                );
        }

        [Test]
        public void ImplicitGlobalVariableSingle()
        {
            RunTest(
                new TestData
                {
                    source = "f = 1.0f",
                    location = new SourceLocation(1, 1),
                    // TODO: should it be 'global variable'?
                    expectedDatatip = "(local variable) f as single",
                    expectedSpan = new TextSpan { iStartLine = 0, iEndLine = 0, iStartIndex = 0, iEndIndex = 1 }
                }
                );
        }

        [Test]
        public void ExplicitGlobalVariableSingle()
        {
            RunTest(
                new TestData
                {
                    source = "f as single\nf = 1.0f",
                    location = new SourceLocation(2, 1),
                    // TODO: should it be 'global variable'?
                    expectedDatatip = "(local variable) f as single",
                    expectedSpan = new TextSpan { iStartLine = 1, iEndLine = 1, iStartIndex = 0, iEndIndex = 1 },
                    expectedDefinition = new TextSpan { iStartLine = 0, iEndLine = 0, iStartIndex = 0, iEndIndex = 1 }
                }
                );
        }

        [Test]
        public void ExplicitGlobalVariable()
        {
            RunTest(
                new TestData
                {
                    source = "a as int\na = 1",
                    location = new SourceLocation(1, 1),
                    expectedDatatip = "",
                    expectedSpan = new TextSpan { iStartLine = 0, iEndLine = 0, iStartIndex = 0, iEndIndex = 0 }
                }
                );
        }

        [Test]
        public void ExplicitGlobalVariableReference()
        {
            RunTest(
                new TestData
                {
                    source = "a as int\na = 1",
                    location = new SourceLocation(2, 1),
                    // TODO: should it be 'global variable'?
                    expectedDatatip = "(local variable) a as int",
                    expectedSpan = new TextSpan { iStartLine = 1, iEndLine = 1, iStartIndex = 0, iEndIndex = 1 },
                    expectedDefinition = new TextSpan { iStartLine = 0, iEndLine = 0, iStartIndex = 0, iEndIndex = 1 }
                }
                );
        }

        [Test]
        public void ParameterReference()
        {
            RunTest(
                new TestData
                {
                    source = "def foo(a as int):\n\tb = a",
                    location = new SourceLocation(2, 9),
                    expectedDatatip = "(parameter) a as int",
                    expectedSpan = new TextSpan { iStartLine = 1, iEndLine = 1, iStartIndex = 5, iEndIndex = 6 },
                    expectedDefinition = new TextSpan { iStartLine = 0, iEndLine = 0, iStartIndex = 8, iEndIndex = 9 }
                }
                );
        }

        [Test]
        public void LocalVariableReference()
        {
            RunTest(
                new TestData
                {
                    source = "def foo(a as int):\n\tb = a",
                    location = new SourceLocation(2, 5),
                    expectedDatatip = "(local variable) b as int",
                    expectedSpan = new TextSpan { iStartLine = 1, iEndLine = 1, iStartIndex = 1, iEndIndex = 2 }
                }
                );
        }

        [Test]
        public void InstanceMemberReferenceExplicit()
        {
            RunTest(
                new TestData
                {
                    source = "class c:\n\tb as int\n\tdef foo(a as int):\n\t\tself.b = a",
                    location = new SourceLocation(4, 14),
                    expectedDatatip = "b as int",
                    expectedSpan = new TextSpan { iStartLine = 3, iEndLine = 3, iStartIndex = 7, iEndIndex = 8 },
                    expectedDefinition = new TextSpan { iStartLine = 1, iEndLine = 1, iStartIndex = 1, iEndIndex = 2 }
                }
                );
        }

        [Test]
        public void ExternalClassMemberReference()
        {
            RunTest(
                new TestData
                {
                    source = "s as string\nl = s.Length",
                    location = new SourceLocation(2, 7),
                    expectedDatatip = "Length as int",
                    expectedSpan = new TextSpan { iStartLine = 1, iEndLine = 1, iStartIndex = 6, iEndIndex = 12 }
                }
                );
        }

        [Test]
        public void InstanceImplicitMemberReferenceExplicit()
        {
            RunTest(
                new TestData
                {
                    source = "class c:\n\tb = 1\n\tdef foo():\n\t\ta=self.b",
                    location = new SourceLocation(4, 16),
                    expectedDatatip = "b as int",
                    expectedSpan = new TextSpan { iStartLine = 3, iEndLine = 3, iStartIndex = 9, iEndIndex = 10 },
                    expectedDefinition = new TextSpan { iStartLine = 1, iEndLine = 1, iStartIndex = 1, iEndIndex = 2 }
                }
                );
        }

        [Test]
        public void InstanceMemberReferenceImplicit()
        {
            RunTest(
                new TestData
                {
                    source = "class c:\n\tb as int\n\tdef foo(a as int):\n\t\tb = a",
                    location = new SourceLocation(4, 9),
                    expectedDatatip = "b as int",
                    expectedSpan = new TextSpan { iStartLine = 3, iEndLine = 3, iStartIndex = 7, iEndIndex = 8 }
                }
                );
        }

        [Test]
        public void LoopVariableReference()
        {
            RunTest(
                new TestData
                {
                    source = "for i in range(5):\n\t a=i",
                    location = new SourceLocation(2, 8),
                    expectedDatatip = "(local variable) i as int",
                    expectedSpan = new TextSpan { iStartLine = 1, iEndLine = 1, iStartIndex = 4, iEndIndex = 5 },
                    expectedDefinition = new TextSpan { iStartLine = 0, iEndLine = 0, iStartIndex = 4, iEndIndex = 5 }
                }
                );
        }
    }
}
