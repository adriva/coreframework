using System;
using System.Collections.Generic;
using System.Linq;
using Adriva.Common.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Adriva.DevTools.CodeGenerator
{
    internal static class Extensions
    {
        private static readonly Dictionary<Type, string> CSharpTypeAliases = new Dictionary<Type, string>
            {
                { typeof(bool), "bool" },
                { typeof(byte), "byte" },
                { typeof(char), "char" },
                { typeof(decimal), "decimal" },
                { typeof(double), "double" },
                { typeof(float), "float" },
                { typeof(int), "int" },
                { typeof(long), "long" },
                { typeof(object), "object" },
                { typeof(sbyte), "sbyte" },
                { typeof(short), "short" },
                { typeof(string), "string" },
                { typeof(uint), "uint" },
                { typeof(ulong), "ulong" },
                { typeof(void), "void" }
            };

        public static NameSyntax ParseCSharpName(this string value) => SyntaxFactory.ParseName(value, 0, true);

        public static TypeSyntax ParseCSharpTypeName(this string value) => SyntaxFactory.ParseTypeName(value, 0, true);

        public static T ApplyModifiers<T>(this T syntaxNode, AccessModifier modifiers) where T : MemberDeclarationSyntax
        {
            List<SyntaxKind> syntaxKinds = new List<SyntaxKind>();

            if (modifiers.HasFlag(AccessModifier.Abstract)) syntaxKinds.Add(SyntaxKind.AbstractKeyword);
            if (modifiers.HasFlag(AccessModifier.Internal)) syntaxKinds.Add(SyntaxKind.InternalKeyword);
            if (modifiers.HasFlag(AccessModifier.Private)) syntaxKinds.Add(SyntaxKind.PrivateKeyword);
            if (modifiers.HasFlag(AccessModifier.Protected)) syntaxKinds.Add(SyntaxKind.ProtectedKeyword);
            if (modifiers.HasFlag(AccessModifier.Public)) syntaxKinds.Add(SyntaxKind.PublicKeyword);
            if (modifiers.HasFlag(AccessModifier.Sealed)) syntaxKinds.Add(SyntaxKind.SealedKeyword);
            if (modifiers.HasFlag(AccessModifier.Partial)) syntaxKinds.Add(SyntaxKind.PartialKeyword);

            if (0 == syntaxKinds.Count) return syntaxNode;

            return (T)syntaxNode.AddModifiers(syntaxKinds.Select(x => SyntaxFactory.Token(x)).ToArray());
        }

        public static T ApplyAttributes<T>(this T syntaxNode, string attributeName, params object[] attributeArguments) where T : MemberDeclarationSyntax
        {
            if (attributeName.EndsWith("Attribute", StringComparison.OrdinalIgnoreCase))
            {
                attributeName = attributeName.Substring(0, attributeName.LastIndexOf("Attribute"));
            }

            var nameSyntax = attributeName.ParseCSharpName();

            var seperatedList = SyntaxFactory.SeparatedList<AttributeSyntax>();
            var attributeList = SyntaxFactory.AttributeList(seperatedList);
            var attribute = SyntaxFactory.Attribute(nameSyntax);

            if (null != attributeArguments && 0 < attributeArguments.Length)
            {
                List<AttributeArgumentSyntax> attributesSyntax = new List<AttributeArgumentSyntax>();

                foreach (var attributeArgument in attributeArguments)
                {
                    var literalExpression = Extensions.CreateLiteralExpression(attributeArgument);
                    var argument = SyntaxFactory.AttributeArgument(literalExpression);
                    attributesSyntax.Add(argument);
                }

                attribute = attribute.AddArgumentListArguments(attributesSyntax.ToArray());
            }

            return (T)syntaxNode.AddAttributeLists(attributeList.AddAttributes(attribute));
        }

        public static LiteralExpressionSyntax CreateLiteralExpression(object value)
        {
            SyntaxToken token;
            if (null == value)
            {
                return SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
            }

            if (value is string stringValue)
            {
                token = SyntaxFactory.Literal(stringValue);
                return SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, token);
            }
            else if (value is bool booleanValue)
            {
                return SyntaxFactory.LiteralExpression(booleanValue ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
            }
            else if (value is int intValue)
            {
                token = SyntaxFactory.Literal(intValue);
                return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, token);
            }
            else if (value is long longValue)
            {
                token = SyntaxFactory.Literal(longValue);
                return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, token);
            }
            else if (value is uint uintValue)
            {
                token = SyntaxFactory.Literal(uintValue);
                return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, token);
            }
            else if (value is ulong ulongValue)
            {
                token = SyntaxFactory.Literal(ulongValue);
                return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, token);
            }
            else if (value is byte byteValue)
            {
                token = SyntaxFactory.Literal(byteValue);
                return SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, token);
            }
            else if (value is char charValue)
            {
                token = SyntaxFactory.Literal(charValue);
                return SyntaxFactory.LiteralExpression(SyntaxKind.CharacterLiteralExpression, token);
            }

            throw new NotSupportedException();
        }

        public static string GetCSharpTypeString(this Type type, bool trimNamespace = false) => type.GetNormalizedName(trimNamespace);

        public static string GetCSharpTypeAlias(this string typeName)
        {
            Type type = Type.GetType(typeName, false);
            if (null == type)
            {
                var matchingType = Extensions.CSharpTypeAliases.Keys.FirstOrDefault(k => 0 == string.CompareOrdinal(k.Name, typeName));
                if (null == matchingType) return typeName;
                type = matchingType;
            }
            return type.GetCSharpTypeAlias();
        }

        public static string GetCSharpTypeAlias(this Type type)
        {
            if (Extensions.CSharpTypeAliases.TryGetValue(type, out string alias)) return alias;
            return type.GetCSharpTypeString();
        }
    }
}
