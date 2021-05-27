namespace Adriva.DevTools.CodeGenerator
{
    public interface IPropertyBuilder : ISyntaxBuilder
    {
        IPropertyBuilder WithName(string propertyName);

        IPropertyBuilder WithType(string typeName);

        IPropertyBuilder WithModifiers(AccessModifier modifiers);

        IPropertyBuilder WithAttribute(string attributeName, params object[] arguments);

        IPropertyBuilder HasGetter(bool hasGetter = true);

        IPropertyBuilder HasSetter(bool hasSetter = true);
    }
}
