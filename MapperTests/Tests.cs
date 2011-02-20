using System;
using System.Collections.Generic;
using System.Reflection;
using Hill30.Boo.ASTMapper;
using Hill30.Boo.ASTMapper.AST.Nodes;
using NUnit.Framework;

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
            Assert.NotNull(mToken);
            Assert.AreEqual(2, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedTypeReference), (mToken.Nodes[1]));
            Assert.AreEqual(Formats.BooType, mToken.Nodes[1].Format);
            Assert.AreEqual("struct int", mToken.Nodes[1].QuickInfoTip);
        }

        [Test]
        public void IntVariableDeclaration()
        {
            var results = RunCompiler(
@"a=1"
                );

            var mToken = results.GetMappedToken(0, 0);
            Assert.NotNull(mToken);
            Assert.AreEqual(2, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedReferenceExpression), (mToken.Nodes[1]));
            Assert.AreEqual("(local variable) a as int", mToken.Nodes[1].QuickInfoTip);
        }

        [Test]
        public void IntVariableReference()
        {
            var results = RunCompiler(
@"a=1
print a"
                );

            var mToken = results.GetMappedToken(1, 6);
            Assert.NotNull(mToken);
            Assert.AreEqual(2, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedReferenceExpression), (mToken.Nodes[1]));
            Assert.AreEqual("(local variable) a as int", mToken.Nodes[1].QuickInfoTip);
        }

        [Test]
        public void MacroReference()
        {
            var results = RunCompiler(
@"a=1
print a"
                );

            var mToken = results.GetMappedToken(1, 0);
            Assert.NotNull(mToken);
            Assert.AreEqual(2, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedMacroReference), (mToken.Nodes[1]));
            Assert.AreEqual(Formats.BooMacro, mToken.Nodes[1].Format);
            Assert.AreEqual("macro print", mToken.Nodes[1].QuickInfoTip);
        }

        [Test]
        public void StringTypeReference()
        {
            var results = RunCompiler(
@"a as string"
                );

            var mToken = results.GetMappedToken(0, 5);
            Assert.NotNull(mToken);
            Assert.AreEqual(2, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedTypeReference), (mToken.Nodes[1]));
            Assert.AreEqual("class string", mToken.Nodes[1].QuickInfoTip);
        }

        [Test]
        public void ClassMemberReference()
        {
            var results = RunCompiler(
@"a as string
print a.Length"
                );

            var mToken = results.GetMappedToken(1, 8);
            Assert.NotNull(mToken);
            Assert.AreEqual(2, mToken.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedReferenceExpression), (mToken.Nodes[1]));
            Assert.AreEqual("Length as int", mToken.Nodes[1].QuickInfoTip);
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
        }
    }
}
