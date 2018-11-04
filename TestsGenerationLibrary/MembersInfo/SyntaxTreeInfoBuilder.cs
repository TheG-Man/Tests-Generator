using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using TestsGenerationLibrary.MembersInfo;

namespace TestsGenerationLibrary.MembersInfo
{
    class SyntaxTreeInfoBuilder
    {
        private readonly string _programText;

        public SyntaxTreeInfoBuilder(string programText)
        {
            _programText = programText;
        }

        public SyntaxTreeInfo GetSyntaxTreeInfo()
        {
            SyntaxTree programSyntaxTree = CSharpSyntaxTree.ParseText(_programText);
            CompilationUnitSyntax root = programSyntaxTree.GetCompilationUnitRoot();

            var classDeclarations = 
                from classDeclaration in root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                select classDeclaration;

            return new SyntaxTreeInfo(GetClasses(classDeclarations));
        }

        private IEnumerable<ClassInfo> GetClasses(IEnumerable<ClassDeclarationSyntax> classDeclarations)
        {
            List<ClassInfo> classes = new List<ClassInfo>();

            foreach (ClassDeclarationSyntax classDeclaration in classDeclarations)
            {
                var constructorDeclarations = 
                    from constructorDeclaration in classDeclaration.DescendantNodes().OfType<ConstructorDeclarationSyntax>()
                    where constructorDeclaration.Modifiers.Any(x => x.ValueText == "public") == true
                    select constructorDeclaration;

                var publicMethods = 
                    from methodDeclaration in classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>()
                    where methodDeclaration.Modifiers.Any(x => x.ValueText == "public") == true
                    select methodDeclaration;

                var namespaceDeclaration = (NamespaceDeclarationSyntax)classDeclaration.Parent;

                string namespaceName = namespaceDeclaration.Name.ToString();
                string className = classDeclaration.Identifier.ValueText;
                ConstructorInfo constructorInfo = GetConstructorInfo(constructorDeclarations);
                IEnumerable<MethodInfo> methods = GetMethods(publicMethods);

                classes.Add(new ClassInfo(namespaceName, className, constructorInfo, methods));
            }

            return classes;
        }

        private ConstructorInfo GetConstructorInfo(IEnumerable<ConstructorDeclarationSyntax> constructorDeclarations)
        {
            if (constructorDeclarations.Any())
            {
                List<ParameterInfo> interfaceParameters = new List<ParameterInfo>();

                IOrderedEnumerable<ConstructorDeclarationSyntax> orderedConstructorDeclarations = constructorDeclarations
                    .OrderByDescending(x => x.ParameterList.Parameters.Count());

                ConstructorDeclarationSyntax constructorDeclaration = constructorDeclarations.FirstOrDefault(x =>
                    x.ParameterList.Parameters.FirstOrDefault(param => param.Type.ToString().IsInterfaceName() == true) != null);

                if (constructorDeclaration == null)
                {
                    constructorDeclaration = orderedConstructorDeclarations.First();
                }
                else
                {
                    foreach (ParameterSyntax parameter in constructorDeclaration.ParameterList.Parameters)
                    {
                        if (parameter.Type.ToString().IsInterfaceName())
                        {
                            interfaceParameters.Add(new ParameterInfo(parameter.Type.ToString(), parameter.Identifier.ValueText));
                        }
                    }
                }

                return new ConstructorInfo(interfaceParameters, GetParameters(constructorDeclaration.ParameterList));
            }
            else
            {
                return null;
            }
        }

        private IEnumerable<MethodInfo> GetMethods(IEnumerable<MethodDeclarationSyntax> methodDeclarations)
        {
            List<MethodInfo> methods = new List<MethodInfo>();

            foreach (MethodDeclarationSyntax methodDeclaration in methodDeclarations)
            {
                string methodName = methodDeclaration.Identifier.ValueText;
                string returnType = methodDeclaration.ReturnType.ToString();
                IEnumerable<ParameterInfo> parameters = GetParameters(methodDeclaration.ParameterList);

                methods.Add(new MethodInfo(methodName, returnType, parameters));
            }

            return methods;
        }

        private IEnumerable<ParameterInfo> GetParameters(ParameterListSyntax parameterList)
        {
            List<ParameterInfo> parameters = new List<ParameterInfo>();

            foreach (ParameterSyntax parameter in parameterList.Parameters)
            {
                parameters.Add(new ParameterInfo(parameter.Type.ToString(), parameter.Identifier.ValueText));
            }

            return parameters;
        }
    }
}
