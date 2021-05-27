using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.DevTools.CodeGenerator
{
    public sealed class CSharpCodeBuilder : ICodeBuilder
    {
        private readonly IServiceProvider ServiceProvider;
        private readonly IList<IClassBuilder> ClassBuilderList = new List<IClassBuilder>();
        private readonly IList<string> UsingNamespaces = new List<string>();

        private string NamespaceName = $"Namespace_{Guid.NewGuid().ToString("N")}";

        public CSharpCodeBuilder(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        public ICodeBuilder AddClass(Action<IClassBuilder> buildClass)
        {
            if (null == buildClass) return this;

            IClassBuilder classBuilder = this.ServiceProvider.GetRequiredService<IClassBuilder>();
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

            var orderedNamespaceDeclerations = this.UsingNamespaces.OrderBy(x => x).Select(x => SyntaxFactory.UsingDirective(x.ParseCSharpName()));

            compilationUnit = compilationUnit.AddUsings(orderedNamespaceDeclerations.ToArray());

            if (0 < this.ClassBuilderList.Count)
            {
                var classDeclerations = this.ClassBuilderList.Select(x => x.Build()).OfType<ClassDeclarationSyntax>().ToArray();
                namespaceDecleration = namespaceDecleration.AddMembers(classDeclerations);
            }

            compilationUnit = compilationUnit.AddMembers(namespaceDecleration);

            return compilationUnit;
        }

        public Task WriteAsync(TextWriter textWriter)
        {
            var syntaxNode = this.Build();
            string code = syntaxNode.NormalizeWhitespace("\t", false).ToFullString();
            return textWriter.WriteAsync(code);
        }
    }
}
