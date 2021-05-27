using Microsoft.CodeAnalysis;

namespace Adriva.DevTools.CodeGenerator
{
    public interface ISyntaxBuilder
    {
        SyntaxNode Build();
    }
}
