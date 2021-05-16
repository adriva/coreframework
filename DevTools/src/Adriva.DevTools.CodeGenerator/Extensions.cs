using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Adriva.DevTools.CodeGenerator
{
    internal static class Extensions
    {
        public static NameSyntax ParseCSharpName(this string value) => SyntaxFactory.ParseName(value, 0, true);

        public static TypeSyntax ParseCSharpTypeName(this string value) => SyntaxFactory.ParseTypeName(value, 0, true);
    }
}
