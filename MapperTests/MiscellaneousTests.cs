using System;
using Boo.Lang.Compiler.Ast;
using Hill30.Boo.ASTMapper;
using Hill30.Boo.ASTMapper.AST.Nodes;
using NUnit.Framework;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Package;

namespace MapperTests
{

    [TestFixture]
    public class MiscellaneousTests : TestBase
    {
        [Test]
        public void MacroReference()
        {
            RunTest(
                new TestData
                {
                    source = "a=1\nprint a",
                    location = new SourceLocation(2, 1),
                    expectedDatatip = "macro print",
                    expectedSpan = new TextSpan { iStartLine = 1, iEndLine = 1, iStartIndex = 0, iEndIndex = 5 }
                }
                );
        }


// TODO: Test the comments mapping
//        [Test]
//        public void Comments()
//        {
//            var results = RunCompiler(
// @"// A comment.
// /* A possibly multiline
// comment. */
// # Another comment"
//                );

//            var mToken = results.GetMappedToken(0, 3);
//            Assert.Null(mToken);

//            results.GetMappedToken(1, 3);
//            Assert.Null(mToken);

//            results.GetMappedToken(2, 2);
//            Assert.Null(mToken);
//        }       
    }
}
