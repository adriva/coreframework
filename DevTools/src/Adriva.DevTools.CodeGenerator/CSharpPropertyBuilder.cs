using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Adriva.DevTools.CodeGenerator
{
    public class CSharpPropertyBuilder : IPropertyBuilder
    {
        private readonly HashSet<Tuple<string, object[]>> AttributeList = new HashSet<Tuple<string, object[]>>();
        private string Name = $"Property_{Guid.NewGuid().ToString("N")}";
        private string TypeName = "object";
        private bool HasGetAccessor = true;
        private bool HasSetAccessor = true;
        private AccessModifier AccessModifiers = AccessModifier.Public;

        public IPropertyBuilder HasGetter(bool hasGetter = true)
        {
            this.HasGetAccessor = hasGetter;
            return this;
        }

        public IPropertyBuilder HasSetter(bool hasSetter = true)
        {
            this.HasSetAccessor = hasSetter;
            return this;
        }

        public IPropertyBuilder WithName(string propertyName)
        {
            this.Name = propertyName;
            return this;
        }

        public IPropertyBuilder WithType(string typeName)
        {
            this.TypeName = typeName.GetCSharpTypeAlias();
            return this;
        }

        public IPropertyBuilder WithModifiers(AccessModifier modifiers)
        {
            this.AccessModifiers = modifiers;
            return this;
        }

        public IPropertyBuilder WithAttribute(string attributeName, params object[] arguments)
        {
            this.AttributeList.Add(new Tuple<string, object[]>(attributeName, arguments));
            return this;
        }

        public SyntaxNode Build()
        {
            TypeSyntax typeSyntax = SyntaxFactory.ParseTypeName(this.TypeName);
            List<AccessorDeclarationSyntax> accessorDeclarations = new List<AccessorDeclarationSyntax>();

            if (this.HasGetAccessor)
            {
                accessorDeclarations.Add(SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
            }

            if (this.HasSetAccessor)
            {
                accessorDeclarations.Add(SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
            }

            if (!this.HasGetAccessor && !this.HasSetAccessor)
            {
                throw new InvalidOperationException();
            }

            var propertyDecleration = SyntaxFactory
                                        .PropertyDeclaration(typeSyntax, this.Name)
                                        .AddAccessorListAccessors(accessorDeclarations.ToArray())
                                        .ApplyModifiers(this.AccessModifiers)
                                        ;

            foreach (var attributeListItem in this.AttributeList)
            {
                propertyDecleration = propertyDecleration.ApplyAttributes(attributeListItem.Item1, attributeListItem.Item2);
            }

            return propertyDecleration;
        }
    }
}
