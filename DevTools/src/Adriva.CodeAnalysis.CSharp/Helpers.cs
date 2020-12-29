using Microsoft.CodeAnalysis;

namespace Adriva.CodeAnalysis.CSharp
{
    internal static class Helpers
    {
        public static TNodeType FindParent<TNodeType>(this SyntaxNode syntaxNode) where TNodeType : SyntaxNode
        {
            var current = syntaxNode;

            while (null != current && !(current is TNodeType))
            {
                current = current.Parent;
            }

            return (TNodeType)current;
        }
    }
}
