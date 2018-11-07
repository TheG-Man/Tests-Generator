using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using TestsGenerationLibrary;
using TestsGenerationLibrary.Consumers;
using TestsGenerationLibrary.SourceCodeProviders;

namespace TestsGeneratoinLibraryTests
{
    [TestFixture]
    public class TestsTemplateGeneratorTests
    {
        private TestsGenerator _testsGenerator;
        private CompilationUnitSyntax _testCompilationUnit;
        private string _fullPath;

        [SetUp]
        public void SetUp()
        {
            _fullPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var fileSourceCodeProvider = new FileSourceCodeProvider(new List<string> { _fullPath + @"\TracerUse.csnotcompilable" });
            var fileConsumer = new FileConsumer(_fullPath + @"\GeneratedTests\");

            _testsGenerator = new TestsGenerator();
            _testsGenerator.Generate(fileSourceCodeProvider, fileConsumer);

            string generatedTestText = File.ReadAllText(_fullPath + @"\GeneratedTests\TracerUseTests.cs");
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(generatedTestText);
            _testCompilationUnit = syntaxTree.GetCompilationUnitRoot();
        }

        [Test]
        public void EntireTestClassTest()
        {
            var expected = File.ReadAllText(_fullPath + @"\GeneratedTestClass.csnotcompilable");
            var actual = File.ReadAllText(_fullPath + @"\GeneratedTests\SecondClassInTheFileTests.cs");

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NUnitUsingDirectiveTest()
        {
            var NUnitUsingDirective =
                from usingDirective in _testCompilationUnit.DescendantNodes().OfType<UsingDirectiveSyntax>()
                where usingDirective.Name.ToString() == "NUnit.Framework"
                select usingDirective;

            Assert.That(NUnitUsingDirective.FirstOrDefault(), Is.Not.Null);
        }

        [Test]
        public void TestTracerNamespaceTest()
        {
            var testTracerTestsNamespace =
                from namespaceDeclaration in _testCompilationUnit.DescendantNodes().OfType<NamespaceDeclarationSyntax>()
                where namespaceDeclaration.Name.ToString() == "TestTracer.Tests"
                select namespaceDeclaration;

            Assert.That(testTracerTestsNamespace.FirstOrDefault(), Is.Not.Null);
        }

        [Test]
        public void TracerUseTestsClassTest()
        {
            var tracerUseTestsClass =
                from classDeclaration in _testCompilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>()
                where classDeclaration.Identifier.ValueText == "TracerUseTests"
                select classDeclaration;

            Assert.That(tracerUseTestsClass.FirstOrDefault(), Is.Not.Null);
        }

        [Test]
        public void TracerUseTestsClassMethodsCountTest()
        {
            var setUpMethod =
                from methodDeclaration in _testCompilationUnit.DescendantNodes().OfType<MethodDeclarationSyntax>()
                select methodDeclaration;

            Assert.That(setUpMethod.Count(), Is.EqualTo(5));
        }

        [Test]
        public void MockFieldPrivateModifierTest()
        {
            var mockField =
                from fieldDeclaration in _testCompilationUnit.DescendantNodes().OfType<FieldDeclarationSyntax>()
                where fieldDeclaration.Modifiers.FirstOrDefault(x => x.ValueText == "private") != null
                select fieldDeclaration;

            Assert.That(mockField.FirstOrDefault(), Is.Not.Null);
        }

        [Test]
        public void MockFieldTypeTest()
        {
            var mockField =
                from fieldDeclaration in _testCompilationUnit.DescendantNodes().OfType<FieldDeclarationSyntax>()
                where fieldDeclaration.Declaration.Type.ToString() == "Mock<ITracer>"
                select fieldDeclaration;

            Assert.That(mockField.FirstOrDefault(), Is.Not.Null);
        }

        [Test]
        public void SetUpMethodTest()
        {
            var setUpMethod =
                from methodDeclaration in _testCompilationUnit.DescendantNodes().OfType<MethodDeclarationSyntax>()
                where methodDeclaration.Identifier.ValueText == "SetUp"
                select methodDeclaration;

            Assert.That(setUpMethod.FirstOrDefault(), Is.Not.Null);
        }

        [Test]
        public void SetUpMethodAttributeTest()
        {
            var setUpMethod =
                from methodDeclaration in _testCompilationUnit.DescendantNodes().OfType<MethodDeclarationSyntax>()
                where methodDeclaration.AttributeLists.FirstOrDefault(x => x.Attributes.FirstOrDefault(
                    attr => attr.Name.ToString() == "SetUp") != null) != null
                select methodDeclaration;

            Assert.That(setUpMethod.FirstOrDefault(), Is.Not.Null);
        }

        [Test]
        public void SetUpMethodStatementsCountTest()
        {
            var setUpMethod =
                from methodDeclaration in _testCompilationUnit.DescendantNodes().OfType<MethodDeclarationSyntax>()
                where methodDeclaration.Identifier.ValueText == "SetUp"
                select methodDeclaration;

            Assert.That(setUpMethod.FirstOrDefault().Body.Statements.Count, Is.EqualTo(4));
        }

        [Test]
        public void SetUpMethodCreateClassStatementsTest()
        {
            var tracerUseCreationExpression =
                from objCreationExpression in _testCompilationUnit.DescendantNodes().OfType<ObjectCreationExpressionSyntax>()
                where objCreationExpression.Type.ToString() == "TracerUse"
                select objCreationExpression;

            Assert.That(tracerUseCreationExpression.FirstOrDefault(), Is.Not.Null);
        }

        [Test]
        public void Method3TestActualVariableTest()
        {
            var setUpMethod =
                from methodDeclaration in _testCompilationUnit.DescendantNodes().OfType<MethodDeclarationSyntax>()
                where methodDeclaration.Identifier.ValueText == "Method3Test"
                select methodDeclaration;

            var actual =
                from actualDeclaration in setUpMethod.FirstOrDefault().Body.Statements.OfType<LocalDeclarationStatementSyntax>()
                where actualDeclaration.Declaration.Variables.FirstOrDefault(x => x.Identifier.ValueText == "actual") != null
                select actualDeclaration;

            Assert.That(actual.FirstOrDefault(), Is.Not.Null);
        }

        [Test]
        public void Method3TestExpectedVariableTest()
        {
            var setUpMethod =
                from methodDeclaration in _testCompilationUnit.DescendantNodes().OfType<MethodDeclarationSyntax>()
                where methodDeclaration.Identifier.ValueText == "Method3Test"
                select methodDeclaration;

            var actual =
                from actualDeclaration in setUpMethod.FirstOrDefault().Body.Statements.OfType<LocalDeclarationStatementSyntax>()
                where actualDeclaration.Declaration.Variables.FirstOrDefault(x => x.Identifier.ValueText == "expected") != null
                select actualDeclaration;

            Assert.That(actual.FirstOrDefault(), Is.Not.Null);
        }

        [Test]
        public void Method3TestAssertFailTest()
        {
            var setUpMethod =
                from methodDeclaration in _testCompilationUnit.DescendantNodes().OfType<MethodDeclarationSyntax>()
                where methodDeclaration.Identifier.ValueText == "Method3Test"
                select methodDeclaration;

            var assert =
                from assertDeclaration in setUpMethod.FirstOrDefault().Body.DescendantNodes().OfType<MemberAccessExpressionSyntax>()
                where assertDeclaration.Expression.ToString() == "Assert"
                select assertDeclaration;

            Assert.That(assert.FirstOrDefault(), Is.Not.Null);
        }
    }
}
