using System;
using Microsoft.CodeAnalysis;

namespace Adriva.DevTools.CodeGenerator
{
    public interface ISyntaxBuilder
    {
        SyntaxNode Build();
    }

    public interface ICodeBuilder : ISyntaxBuilder
    {
        ICodeBuilder AddUsingStatement(string namespaceName);

        ICodeBuilder WithNamespace(string namespaceName);

        ICodeBuilder AddClass(Action<IClassBuilder> buildClass);
    }

    public interface IClassBuilder : ISyntaxBuilder
    {
        IClassBuilder WithName(string className);

        IClassBuilder AddProperty(Action<IPropertyBuilder> propertyBuilder);


    }

    public interface IPropertyBuilder
    {
        IPropertyBuilder WithName(string propertyName);

        IPropertyBuilder WithType(string typeName);

        IPropertyBuilder HasGetter(bool hasGetter = true);

        IPropertyBuilder HasSetter(bool hasSetter = true);
    }
}
