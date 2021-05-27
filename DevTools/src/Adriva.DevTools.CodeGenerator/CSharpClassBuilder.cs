using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.DevTools.CodeGenerator
{

    public sealed class CSharpClassBuilder : IClassBuilder
    {
        private readonly IServiceProvider ServiceProvider;
        private readonly IList<IPropertyBuilder> PropertyBuilderList = new List<IPropertyBuilder>();
        private readonly IList<string> BaseTypesList = new List<string>();
        private readonly HashSet<Tuple<string, object[]>> AttributeList = new HashSet<Tuple<string, object[]>>();

        private string ClassName = $"Class_{Guid.NewGuid().ToString("N")}";
        private AccessModifier Modifiers = AccessModifier.None;

        public CSharpClassBuilder(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        public IClassBuilder WithName(string className)
        {
            this.ClassName = className;
            return this;
        }

        public IClassBuilder WithModifiers(AccessModifier modifiers)
        {
            this.Modifiers = modifiers;
            return this;
        }

        public IClassBuilder WithBaseType(string typeName)
        {
            this.BaseTypesList.Add(typeName);
            return this;
        }

        public IClassBuilder WithProperty(Action<IPropertyBuilder> buildProperty)
        {
            IPropertyBuilder propertyBuilder = this.ServiceProvider.GetRequiredService<IPropertyBuilder>();
            buildProperty(propertyBuilder);
            this.PropertyBuilderList.Add(propertyBuilder);
            return this;
        }

        public IClassBuilder WithAttribute(string attributeName, params object[] arguments)
        {
            this.AttributeList.Add(new Tuple<string, object[]>(attributeName, arguments));
            return this;
        }

        public SyntaxNode Build()
        {
            var classDecleration = SyntaxFactory.ClassDeclaration(this.ClassName);

            classDecleration = classDecleration.ApplyModifiers(this.Modifiers);

            if (0 < this.BaseTypesList.Count)
            {
                var baseTypeDeclerations = this.BaseTypesList.Select(x => SyntaxFactory.SimpleBaseType(x.ParseCSharpTypeName())).ToArray();
                classDecleration = classDecleration.AddBaseListTypes(baseTypeDeclerations);
            }

            if (0 < this.PropertyBuilderList.Count)
            {
                var propertyDeclerations = this.PropertyBuilderList.Select(b => b.Build()).OfType<PropertyDeclarationSyntax>().ToArray();
                classDecleration = classDecleration.AddMembers(propertyDeclerations);
            }

            foreach (var attributeListItem in this.AttributeList)
            {
                classDecleration = classDecleration.ApplyAttributes(attributeListItem.Item1, attributeListItem.Item2);
            }

            return classDecleration;
        }
    }
}
