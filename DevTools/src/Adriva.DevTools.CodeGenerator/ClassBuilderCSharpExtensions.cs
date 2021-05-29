using System;

namespace Adriva.DevTools.CodeGenerator
{
    public static class ClassBuilderCSharpExtensions
    {
        public static IClassBuilder WithBaseType(this IClassBuilder classBuilder, Type type, bool ignoreNamespace = false)
        {
            return classBuilder.WithBaseType(type.GetCSharpTypeString(ignoreNamespace));
        }

        public static IClassBuilder WithAttribute<TAttribute>(this IClassBuilder classBuilder, bool ignoreNamespace = false, params object[] arguments) where TAttribute : Attribute
        {
            Type typeOfAttribute = typeof(TAttribute);
            string attributeName = typeOfAttribute.GetCSharpTypeString(ignoreNamespace);
            return classBuilder.WithAttribute(attributeName, arguments);
        }

    }
}
