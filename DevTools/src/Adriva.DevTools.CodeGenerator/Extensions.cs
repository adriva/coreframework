using System;
using System.Collections.Generic;
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
    }
}
