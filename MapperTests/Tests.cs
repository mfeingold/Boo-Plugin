using System;
using System.Collections.Generic;
using System.Reflection;
using Hill30.Boo.ASTMapper;
using Hill30.Boo.ASTMapper.AST.Nodes;
using NUnit.Framework;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Package;

namespace MapperTests
{

    [TestFixture]
    public class Tests
    {
        private static CompileResults RunCompiler(string source)
        {
            var results = new CompileResults(
                () => "Test",
                () => source,
                () => 4
                );
            CompilerManager.Compile(new Assembly[] { typeof(SerializableAttribute).Assembly }, new[] { results });
            return results;
        }

        [Test]
        public void IntTypeReference()
        {
            var results = RunCompiler(
@"a as int"
                );

            var mToken = results.GetMappedToken(0, 5);
            TextSpan ts = new TextSpan();
            TextSpan expected = new TextSpan();
            expected.iStartLine = 0;
            expected.iEndLine = 0;
            expected.iStartIndex = 5;
            expected.iEndIndex = 8;

            Assert.NotNull(mToken);
            Assert.AreEqual(2, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedTypeReference), (mToken.Nodes[1]));
            Assert.AreEqual(Formats.BooType, mToken.Nodes[1].Format);
            Assert.AreEqual("struct int", mToken.GetDataTiptext(out ts));
            Assert.AreEqual(ts, expected);
        }

        [Test]
        public void IntVariableDeclaration()
        {
            var results = RunCompiler(
@"a=1"
                );

            var mToken = results.GetMappedToken(0, 0);
            TextSpan ts = new TextSpan();
            TextSpan expected = new TextSpan();
            expected.iStartLine = 0;
            expected.iEndLine = 0;
            expected.iStartIndex = 0;
            expected.iEndIndex = 1;

            Assert.NotNull(mToken);
            Assert.AreEqual(2, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedReferenceExpression), (mToken.Nodes[1]));
            Assert.AreEqual("(local variable) a as int", mToken.GetDataTiptext(out ts));
            Assert.AreEqual(ts, expected);
        }

        [Test]
        public void IntVariableReference()
        {
            var results = RunCompiler(
@"a=1
print a"
                );

            var mToken = results.GetMappedToken(1, 6);
            TextSpan ts = new TextSpan();
            TextSpan expected = new TextSpan();
            expected.iStartLine = 1;
            expected.iEndLine = 1;
            expected.iStartIndex = 6;
            expected.iEndIndex = 7;

            Assert.NotNull(mToken);
            Assert.AreEqual(2, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedReferenceExpression), (mToken.Nodes[1]));
            Assert.AreEqual("(local variable) a as int", mToken.GetDataTiptext(out ts));
            Assert.AreEqual(ts, expected);
        }

        [Test]
        public void MacroReference()
        {
            var results = RunCompiler(
@"a=1
print a"
                );

            var mToken = results.GetMappedToken(1, 0);
            TextSpan ts = new TextSpan();
            TextSpan expected = new TextSpan();
            expected.iStartLine = 1;
            expected.iEndLine = 1;
            expected.iStartIndex = 0;
            expected.iEndIndex = 5;

            Assert.NotNull(mToken);
            Assert.AreEqual(2, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedMacroReference), (mToken.Nodes[1]));
            Assert.AreEqual(Formats.BooMacro, mToken.Nodes[1].Format);
            Assert.AreEqual("macro print", mToken.GetDataTiptext(out ts));
            Assert.AreEqual(ts, expected);
        }

        [Test]
        public void StringTypeReference()
        {
            var results = RunCompiler(
@"a as string"
                );

            var mToken = results.GetMappedToken(0, 5);
            TextSpan ts = new TextSpan();
            TextSpan expected = new TextSpan();
            expected.iStartLine = 0;
            expected.iEndLine = 0;
            expected.iStartIndex = 5;
            expected.iEndIndex = 11;

            Assert.NotNull(mToken);
            Assert.AreEqual(2, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedTypeReference), (mToken.Nodes[1]));
            Assert.AreEqual("class string", mToken.GetDataTiptext(out ts));
            Assert.AreEqual(ts, expected);
        }

        [Test]
        public void BoolTypeReference()
        {
            var results = RunCompiler(
@"a as bool"
                );

            var mToken = results.GetMappedToken(0, 5);
            TextSpan ts = new TextSpan();
            TextSpan expected = new TextSpan();
            expected.iStartLine = 0;
            expected.iEndLine = 0;
            expected.iStartIndex = 5;
            expected.iEndIndex = 9;


            Assert.NotNull(mToken);
            Assert.AreEqual(2, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedTypeReference), (mToken.Nodes[1]));
            Assert.AreEqual("struct bool", mToken.GetDataTiptext(out ts));
            Assert.AreEqual(ts, expected);

            mToken.Goto(out ts);
            TokenInfo ti = new TokenInfo();
            ParseReason pr = new ParseReason();
            mToken.GetDeclarations(ti, pr);

        }

        [Test]
        public void CharVariableDeclaration()
        {
            var results = RunCompiler(
@"c = char('a')"
                );

            var mToken = results.GetMappedToken(0, 0);
            TextSpan ts = new TextSpan();
            TextSpan expected = new TextSpan();
            expected.iStartLine = 0;
            expected.iEndLine = 0;
            expected.iStartIndex = 0;
            expected.iEndIndex = 1;

            Assert.NotNull(mToken);
            Assert.AreEqual(2, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedReferenceExpression), (mToken.Nodes[1]));
            Assert.AreEqual("(local variable) c as char", mToken.GetDataTiptext(out ts));
            Assert.AreEqual(ts, expected);

            mToken.Goto(out ts);
            TokenInfo ti = new TokenInfo();
            ParseReason pr = new ParseReason();
            mToken.GetDeclarations(ti, pr);
        }

        [Test]
        public void FloatVariableDeclaration()
        {
            var results = RunCompiler(
//@"f as single" // does not work
@"f = 1.0f"
                );

            var mToken = results.GetMappedToken(0, 0);
            TextSpan ts = new TextSpan();
            TextSpan expected = new TextSpan();
            expected.iStartLine = 0;
            expected.iEndLine = 0;
            expected.iStartIndex = 0;
            expected.iEndIndex = 1;

            Assert.NotNull(mToken);
            Assert.AreEqual(2, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedReferenceExpression), (mToken.Nodes[1]));
            Assert.AreEqual("(local variable) f as single", mToken.GetDataTiptext(out ts));
            Assert.AreEqual(ts, expected);

            mToken.Goto(out ts);
            TokenInfo ti = new TokenInfo();
            ParseReason pr = new ParseReason();
            mToken.GetDeclarations(ti, pr);
        }


        [Test]
        public void ClassMemberReference()
        {
            var results = RunCompiler(
@"a as string
print a.Length"
                );

            var mToken = results.GetMappedToken(1, 8);
            TextSpan ts = new TextSpan();
            TextSpan expected = new TextSpan();
            expected.iStartLine = 1;
            expected.iEndLine = 1;
            expected.iStartIndex = 8;
            expected.iEndIndex = 14;

            Assert.NotNull(mToken);
            Assert.AreEqual(2, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedReferenceExpression), (mToken.Nodes[1]));
            Assert.AreEqual("Length as int", mToken.GetDataTiptext(out ts));
            Assert.AreEqual(ts, expected);
        }

        [Test]
        public void Attribute()
        {
            var results = RunCompiler(
@"[System.Serializable]
class Class:
	def constructor():
		pass"
                );

            var mToken = results.GetMappedToken(0, 1);
            Assert.NotNull(mToken);
            Assert.AreEqual(2, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedAttribute), (mToken.Nodes[1]));
            Assert.AreEqual(Formats.BooType, mToken.Nodes[1].Format);
//            Assert.AreEqual("class System.Serializable", mToken.Nodes[1].QuickInfoTip);
            TextSpan ts = new TextSpan();

            mToken.Goto(out ts);
            TokenInfo ti = new TokenInfo();
            ParseReason pr = new ParseReason();
            mToken.GetDeclarations(ti, pr);
        }

        [Test]
        public void ClassTypeDefinition()
        {
            var results = RunCompiler(
@"class Foo:
    x as int"
                );

            var mToken = results.GetMappedToken(0, 6);
            Assert.NotNull(mToken);
            Assert.AreEqual(2, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedTypeDefinition), (mToken.Nodes[1]));
        }

        [Test]
        public void FinalTypeDefinition()
        {
            var results = RunCompiler(
@"class Foo:
    final x as int"
                );

            var mToken = results.GetMappedToken(1, 4);
            Assert.NotNull(mToken);
            Assert.AreEqual(2, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedTypeDefinition), (mToken.Nodes[1]));
        }

        [Test]
        public void Import()
        {
            var results = RunCompiler(
@"import System
import System.Windows.Forms as SWF

x = SWF.Form()"
            );

            var mToken = results.GetMappedToken(0, 0);
            TextSpan ts = new TextSpan();

            Assert.NotNull(mToken);
            Assert.AreEqual(1, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedTypeDefinition), (mToken.Nodes[0]));
        }

        [Test]
        public void FormVariableDeclaration()
        {
            var results = RunCompiler(
@"import System
import System.Windows.Forms as SWF

x = SWF.Form()"
            );

            var mToken = results.GetMappedToken(3, 0);
            TextSpan ts = new TextSpan();
            TextSpan expected = new TextSpan();
            expected.iStartLine = 3;
            expected.iEndLine = 3;
            expected.iStartIndex = 0;
            expected.iEndIndex = 1;

            Assert.NotNull(mToken);
            Assert.AreEqual(2, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedReferenceExpression), (mToken.Nodes[1]));
            Assert.AreEqual("(local variable) x as System.Windows.Forms.Form", mToken.GetDataTiptext(out ts));
            Assert.AreEqual(ts, expected);
        }

        [Test]
        public void XmlDocumentVariableDeclaration()
        {
            var results = RunCompiler(
@"import System
import System.Xml

doc = XmlDocument()"
            );

            var mToken = results.GetMappedToken(3, 0);
            TextSpan ts = new TextSpan();
            TextSpan expected = new TextSpan();
            expected.iStartLine = 3;
            expected.iEndLine = 3;
            expected.iStartIndex = 0;
            expected.iEndIndex = 3;

            Assert.NotNull(mToken);
            Assert.AreEqual(2, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedReferenceExpression), (mToken.Nodes[1]));
            Assert.AreEqual("(local variable) doc as System.Xml.XmlDocument", mToken.GetDataTiptext(out ts));
            Assert.AreEqual(ts, expected);
        }


        [Test]
        public void IntStaicVariableDeclaration()
        {
            var results = RunCompiler(
@"class Foo:
    static final x = 3"
                );

            var mToken = results.GetMappedToken(1, 17);
            TextSpan ts = new TextSpan();
            TextSpan expected = new TextSpan();
            expected.iStartLine = 1;
            expected.iEndLine = 1;
            expected.iStartIndex = 17;
            expected.iEndIndex = 18;

            Assert.NotNull(mToken);
            Assert.AreEqual(3, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedTypeMemberDefinition), (mToken.Nodes[2]));
            Assert.AreEqual("(local variable) x as int", mToken.GetDataTiptext(out ts));
            Assert.AreEqual(ts, expected);
        }

        [Test]
        public void IntVariableAtLoopDeclaration()
        {
            var results = RunCompiler(
@"for i in range(5):
    print i"
                );

            var mToken = results.GetMappedToken(1, 10);
            TextSpan ts = new TextSpan();
            TextSpan expected = new TextSpan();
            expected.iStartLine = 1;
            expected.iEndLine = 1;
            expected.iStartIndex = 10;
            expected.iEndIndex = 11;

            Assert.NotNull(mToken);
            Assert.AreEqual(2, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedReferenceExpression), (mToken.Nodes[1]));
            Assert.AreEqual("(local variable) i as int", mToken.GetDataTiptext(out ts));
            Assert.AreEqual(ts, expected);
        }

        [Test]
        public void Comments()
        {
            var results = RunCompiler(
@"// A comment.
/* A possibly multiline
comment. */
# Another comment"
                );

            var mToken = results.GetMappedToken(0, 3);
            Assert.Null(mToken);

            results.GetMappedToken(1, 3);
            Assert.Null(mToken);

            results.GetMappedToken(2, 2);
            Assert.Null(mToken);
        }

        [Test]
        public void ForLoop()
        {
            var results = RunCompiler(
@"for i in range(10):
    continue if i % 2 == 0
    print i"
                );

            var mToken = results.GetMappedToken(1, 16);
            TextSpan ts = new TextSpan();
            TextSpan expected = new TextSpan();
            expected.iStartLine = 1;
            expected.iEndLine = 1;
            expected.iStartIndex = 16;
            expected.iEndIndex = 17;

            Assert.NotNull(mToken);
            Assert.AreEqual(2, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedReferenceExpression), (mToken.Nodes[1]));
            Assert.AreEqual("(local variable) i as int", mToken.GetDataTiptext(out ts));
            Assert.AreEqual(ts, expected);
        }

        [Test]
        public void ForLoopContinue()
        {
            var results = RunCompiler(
@"for i in range(10):
    continue if i % 2 == 0
    print i"
                );

            var mToken = results.GetMappedToken(1, 4);
            //TextSpan ts = new TextSpan();

            Assert.NotNull(mToken);
            Assert.AreEqual(1, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedTypeDefinition), (mToken.Nodes[0]));
            //Assert.AreEqual("", mToken.GetDataTiptext(out ts));

        }

        [Test]
        public void ForLoopRange()
        {
            var results = RunCompiler(
@"for i in range(10):
    continue if i % 2 == 0
    print i"
                );

            var mToken = results.GetMappedToken(0, 15);
            //TextSpan ts = new TextSpan();

            Assert.NotNull(mToken);
            Assert.AreEqual(1, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedTypeDefinition), (mToken.Nodes[0]));
            //Assert.AreEqual("", mToken.GetDataTiptext(out ts));

        }
        
    }
}
