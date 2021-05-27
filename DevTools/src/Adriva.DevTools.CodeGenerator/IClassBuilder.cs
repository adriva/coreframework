using System;

namespace Adriva.DevTools.CodeGenerator
{
    public interface IClassBuilder : ISyntaxBuilder
    {
        IClassBuilder WithName(string className);

        IClassBuilder WithModifiers(AccessModifier modifiers);

        IClassBuilder WithBaseType(string typeName);

        IClassBuilder WithProperty(Action<IPropertyBuilder> buildProperty);

        IClassBuilder WithAttribute(string attributeName, params object[] arguments);
    }
}
