using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Adriva.DevTools.CodeGenerator
{

    public sealed class CSharpClassBuilder : IClassBuilder
    {
        private readonly IList<IPropertyBuilder> PropertyBuilderList = new List<IPropertyBuilder>();
        private readonly IList<string> BaseTypesList = new List<string>();

        private string ClassName = $"Class_{Guid.NewGuid().ToString("N")}";
        private AccessModifier Modifiers = AccessModifier.None;

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

        public IClassBuilder AddBaseType(string typeName)
        {
            this.BaseTypesList.Add(typeName);
            return this;
        }

        public IClassBuilder AddProperty(Action<IPropertyBuilder> propertyBuilder)
        {
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

            return classDecleration;
        }
    }
}
