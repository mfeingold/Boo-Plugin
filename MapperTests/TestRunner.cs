using System;
using System.Collections.Generic;
using System.Reflection;
using Hill30.Boo.ASTMapper;
using Hill30.Boo.ASTMapper.AST.Nodes;
using NUnit.Framework;

namespace MapperTests
{

    [TestFixture]
    public class MapperTests
    {
        private static CompileResults RunCompiler(string source)
        {
            var results = new CompileResults(
                () => "Test",
                r => source,
                () => 4
                );
            results.Initialize();
            CompilerManager.Compile(new Assembly[] { }, new[] { results });
            return results;
        }

        [Test]
        public void IntVariableDeclarationQTip()
        {
            var results = RunCompiler(
@"a=1"
                );

            var varADeclaration = results.GetMappedToken(0, 0);
            Assert.NotNull(varADeclaration);
            Assert.AreEqual(2, varADeclaration.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedReferenceExpression), (varADeclaration.Nodes[1]));
            Assert.AreEqual("(local variable) a as int", varADeclaration.Nodes[1].QuickInfoTip);
        }

        [Test]
        public void IntVariableReferenceQTip()
        {
            var results = RunCompiler(
@"a=1
print a"
                );

            var varADeclaration = results.GetMappedToken(1, 6);
            Assert.NotNull(varADeclaration);
            Assert.AreEqual(2, varADeclaration.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedReferenceExpression), (varADeclaration.Nodes[1]));
            Assert.AreEqual("(local variable) a as int", varADeclaration.Nodes[1].QuickInfoTip);
        }

        [Test]
        public void MacroReferenceQTip()
        {
            var results = RunCompiler(
@"a=1
print a"
                );

            var varADeclaration = results.GetMappedToken(1, 0);
            Assert.NotNull(varADeclaration);
            Assert.AreEqual(2, varADeclaration.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedMacroReference), (varADeclaration.Nodes[1]));
            Assert.AreEqual("macro print", varADeclaration.Nodes[1].QuickInfoTip);
            varADeclaration = results.GetMappedToken(1, 4);
            Assert.NotNull(varADeclaration);
            Assert.AreEqual(2, varADeclaration.Nodes.Count);
            Assert.IsInstanceOf(typeof(MappedMacroReference), (varADeclaration.Nodes[1]));
            Assert.AreEqual("macro print", varADeclaration.Nodes[1].QuickInfoTip);
        }
    }

}
