using System.Collections.Generic;
using System.Linq;
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
    }
}
