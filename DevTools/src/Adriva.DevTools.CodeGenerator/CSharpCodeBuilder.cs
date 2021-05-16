using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Adriva.DevTools.CodeGenerator
{
    public sealed class CSharpCodeBuilder : ICodeBuilder
    {
        private readonly IList<IClassBuilder> ClassBuilderList = new List<IClassBuilder>();
        private readonly IList<string> UsingNamespaces = new List<string>();

        private string NamespaceName = $"Namespace_{Guid.NewGuid().ToString("N")}";

        public ICodeBuilder AddClass(Action<IClassBuilder> buildClass)
        {
            if (null == buildClass) return this;

            IClassBuilder classBuilder = new CSharpClassBuilder();
            buildClass(classBuilder);
            this.ClassBuilderList.Add(classBuilder);
            return this;
        }

        public ICodeBuilder AddUsingStatement(string namespaceName)
        {
            this.UsingNamespaces.Add(namespaceName);
            return this;
        }

        public ICodeBuilder WithNamespace(string namespaceName)
        {
            this.NamespaceName = namespaceName;
            return this;
        }

        public SyntaxNode Build()
        {
            CompilationUnitSyntax compilationUnit = SyntaxFactory.CompilationUnit();

            var namespaceDecleration = SyntaxFactory.NamespaceDeclaration(this.NamespaceName.ParseCSharpName());

            compilationUnit = compilationUnit.AddMembers(namespaceDecleration);

            var orderedNamespaceDeclerations = this.UsingNamespaces.OrderBy(x => x).Select(x => SyntaxFactory.UsingDirective(x.ParseCSharpName()));

            compilationUnit = compilationUnit.AddUsings(orderedNamespaceDeclerations.ToArray());

            System.Console.WriteLine(compilationUnit.NormalizeWhitespace("\t", false).ToFullString());

            return compilationUnit;
        }
    }

    public sealed class CSharpClassBuilder : IClassBuilder
    {
        private readonly IList<IPropertyBuilder> PropertyBuilderList = new List<IPropertyBuilder>();

        private string ClassName = $"Class_{Guid.NewGuid().ToString("N")}";

        public IClassBuilder WithName(string className)
        {
            this.ClassName = className;
            return this;
        }

        public IClassBuilder AddProperty(Action<IPropertyBuilder> propertyBuilder)
        {
            throw new NotImplementedException();
        }

        public SyntaxNode Build()
        {
            return null;
        }
    }
}
