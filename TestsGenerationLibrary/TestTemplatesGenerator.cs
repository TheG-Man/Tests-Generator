using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using TestsGenerationLibrary.MembersInfo;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace TestsGenerationLibrary
{
    class TestTemplatesGenerator
    {
        private readonly SyntaxTreeInfo _syntaxTreeInfo;

        public TestTemplatesGenerator(SyntaxTreeInfo syntaxTreeInfo)
        {
            _syntaxTreeInfo = syntaxTreeInfo;
        }

        public IEnumerable<TestClassInMemoryInfo> GetTestTemplates()
        {
            List<TestClassInMemoryInfo> testTemplates = new List<TestClassInMemoryInfo>();

            foreach (ClassInfo classInfo in _syntaxTreeInfo.Classes)
            {
                NamespaceDeclarationSyntax namespaceDeclaration = NamespaceDeclaration(QualifiedName(
                    IdentifierName(classInfo.NamespaceName), IdentifierName("Tests")));

                CompilationUnitSyntax compilationUnit = CompilationUnit()
                    .WithUsings(GetUsingDirectives(classInfo))
                    .WithMembers(SingletonList<MemberDeclarationSyntax>(namespaceDeclaration
                        .WithMembers(SingletonList<MemberDeclarationSyntax>(ClassDeclaration(classInfo.Name + "Tests")
                            .WithAttributeLists(SingletonList(AttributeList(SingletonSeparatedList(Attribute(IdentifierName("TestFixture"))))))
                            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                            .WithMembers(GetClassMembers(classInfo))))));

                string fileName = $"{classInfo.Name}Tests.cs";
                string fileData = compilationUnit.NormalizeWhitespace().ToFullString();

                testTemplates.Add(new TestClassInMemoryInfo(fileName, fileData));
            }

            return testTemplates;
        }

        private SyntaxList<UsingDirectiveSyntax> GetUsingDirectives(ClassInfo classInfo)
        {
            List<UsingDirectiveSyntax> usingDirectives = new List<UsingDirectiveSyntax>
            {
                UsingDirective(IdentifierName("System")),
                UsingDirective(QualifiedName(
                QualifiedName(IdentifierName("System"), IdentifierName("Collections")), IdentifierName("Generic"))
            ),
                UsingDirective(QualifiedName(IdentifierName("System"), IdentifierName("Linq"))),
                UsingDirective(QualifiedName(IdentifierName("System"), IdentifierName("Text"))),
                UsingDirective(QualifiedName(IdentifierName("NUnit"), IdentifierName("Framework"))),
                UsingDirective(IdentifierName(classInfo.NamespaceName))
            };

            if (classInfo.Constructor != null)
            {
                if (classInfo.Constructor.InterfaceParameters.Any())
                {
                    usingDirectives.Add(UsingDirective(IdentifierName("Moq")));
                }
            }

            return List(usingDirectives);
        }

        private SyntaxList<MemberDeclarationSyntax> GetClassMembers(ClassInfo classInfo)
        {
            List<MemberDeclarationSyntax> classMembers = new List<MemberDeclarationSyntax>();
            
            if (classInfo.Constructor != null)
            {
                classMembers.Add(GetFieldDeclaration(classInfo.Name, classInfo.Name.ToFieldName()));

                if (classInfo.Constructor.InterfaceParameters.Any())
                {
                    foreach (ParameterInfo parameter in classInfo.Constructor.InterfaceParameters)
                    {
                        classMembers.Add(GetFieldDeclarationWithGenericType("Mock", parameter.TypeName, parameter.Name.ToFieldName()));
                    }
                }

                classMembers.Add(GetSetUpMethodDeclaration(classInfo));
            }

            foreach (MethodInfo methodInfo in classInfo.Methods)
            {
                classMembers.Add(GetTestMethodDeclaration(methodInfo, classInfo.Name.ToFieldName()));
            }

            return List(classMembers);
        }

        private MethodDeclarationSyntax GetMethodDeclaration(string attributeName, string methodName, SyntaxList<StatementSyntax> blockMembers)
        {
            MethodDeclarationSyntax methodDeclaration = MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier(methodName))
                .WithAttributeLists(SingletonList(AttributeList(SingletonSeparatedList(Attribute(IdentifierName(attributeName))))))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithBody(Block(blockMembers));

            return methodDeclaration;
        }

        private MethodDeclarationSyntax GetSetUpMethodDeclaration(ClassInfo classInfo)
        {
            List<StatementSyntax> blockMembers = new List<StatementSyntax>();
            List<ArgumentSyntax> parameters = new List<ArgumentSyntax>();

            if (classInfo.Constructor.InterfaceParameters.Any())
            {
                foreach (ParameterInfo parameter in classInfo.Constructor.InterfaceParameters)
                {
                    parameters.Add(Argument(GetMemberAccessExpression(parameter.Name.ToFieldName(), "Object")));
                    var rightExpr = GetObjectCreationExpression("Mock", new List<string> { parameter.TypeName }, ArgumentList());
                    blockMembers.Add(ExpressionStatement(GetAssignmentExpression(parameter.Name.ToFieldName(), rightExpr)));
                }
            }

            foreach (ParameterInfo parameter in classInfo.Constructor.Parameters)
            {
                if (!parameter.TypeName.IsInterfaceName())
                {
                    parameters.Add(Argument(IdentifierName(parameter.Name)));
                    blockMembers.Add(GetLocalDeclarationStatement(parameter.Name, parameter.TypeName, 
                        DefaultExpression(IdentifierName(parameter.TypeName))));
                }
            }            
           
            var rightObjCreationExpr = GetObjectCreationExpression(classInfo.Name, new List<string>(), ArgumentList(SeparatedList(parameters)));
            blockMembers.Add(ExpressionStatement(GetAssignmentExpression(classInfo.Name.ToFieldName(), rightObjCreationExpr)));

            return GetMethodDeclaration("SetUp", "SetUp", List(blockMembers));
        }

        private MethodDeclarationSyntax GetTestMethodDeclaration(MethodInfo methodInfo, string className)
        {
            List<StatementSyntax> blockMembers = new List<StatementSyntax>();
            List<ArgumentSyntax> parameters = new List<ArgumentSyntax>();

            if (methodInfo.ReturnTypeName != "void")
            {
                foreach (ParameterInfo parameter in methodInfo.Parameters)
                {
                    parameters.Add(Argument(IdentifierName(parameter.Name)));
                    blockMembers.Add(GetLocalDeclarationStatement(parameter.Name, parameter.TypeName, 
                        DefaultExpression(IdentifierName(parameter.TypeName))));
                }

                var expression = InvocationExpression(GetMemberAccessExpression(className, methodInfo.Name))
                    .WithArgumentList(ArgumentList(SeparatedList(parameters)));
                blockMembers.Add(GetLocalDeclarationStatement("actual", methodInfo.ReturnTypeName, expression));

                blockMembers.Add(GetLocalDeclarationStatement("expected", methodInfo.ReturnTypeName, 
                    DefaultExpression(IdentifierName(methodInfo.ReturnTypeName))));

                List<ArgumentSyntax> methodParams = new List<ArgumentSyntax>
                {
                    Argument(IdentifierName("actual")),
                    Argument(InvocationExpression(GetMemberAccessExpression("Is", "EqualTo"))
                        .WithArgumentList(ArgumentList(SingletonSeparatedList(Argument(IdentifierName("expected"))))))
                };

                blockMembers.Add(ExpressionStatement(InvocationExpression(GetMemberAccessExpression("Assert", "That"))
                    .WithArgumentList(ArgumentList(SeparatedList(methodParams)))));
            }

            var args = ArgumentList(SingletonSeparatedList(Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("autogenerated")))));
            blockMembers.Add(ExpressionStatement(InvocationExpression(GetMemberAccessExpression("Assert", "Fail")).WithArgumentList(args)));

            return GetMethodDeclaration("Test", $"{methodInfo.Name}Test", List(blockMembers));
        }

        private AssignmentExpressionSyntax GetAssignmentExpression(string identifier, ExpressionSyntax right)
        {
            AssignmentExpressionSyntax assignmentExpression = AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                IdentifierName(identifier),
                right);

            return assignmentExpression;
        }
        
        private MemberAccessExpressionSyntax GetMemberAccessExpression(string objectName, string memberName)
        {
            return MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName(objectName), IdentifierName(memberName));
        }

        private FieldDeclarationSyntax GetFieldDeclaration(string typeName, string name)
        {
            return FieldDeclaration(VariableDeclaration(IdentifierName(typeName))
                .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(name)))))
                .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));
        }

        private FieldDeclarationSyntax GetFieldDeclarationWithGenericType(string typeName, string genericType, string name)
        {
            return FieldDeclaration(VariableDeclaration(GetGenericName(typeName, new List<string> { genericType }))
                .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(name)))))
                .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));
        }

        private ObjectCreationExpressionSyntax GetObjectCreationExpression(string identifier, List<string> typeArgumentList, ArgumentListSyntax arguments)
        {
            NameSyntax name;

            if (typeArgumentList.Any())
            {
                name = GetGenericName(identifier, typeArgumentList);
            }
            else
            {
                name = IdentifierName(identifier);
            }

           ObjectCreationExpressionSyntax objectCreationExpression = ObjectCreationExpression(name)
                .WithArgumentList(arguments);

            return objectCreationExpression;
        }

        private GenericNameSyntax GetGenericName(string identifier, List<string> typeArgumentList)
        {
            List<TypeSyntax> typeArguments = new List<TypeSyntax>();
            
            foreach (string typeArgument in typeArgumentList)
            {
                typeArguments.Add(IdentifierName(typeArgument));
            }
            
            GenericNameSyntax genericName = GenericName(Identifier(identifier))
                .WithTypeArgumentList(TypeArgumentList(SeparatedList(typeArguments)));

            return genericName;
        }

        private LocalDeclarationStatementSyntax GetLocalDeclarationStatement(string identifier, string typeName, ExpressionSyntax expression)
        {
            return LocalDeclarationStatement(VariableDeclaration(IdentifierName(typeName))
                .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(identifier))
                    .WithInitializer(EqualsValueClause(expression)))));
        }
    }
}
