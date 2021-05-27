using System;
using System.IO;
using System.Threading.Tasks;

namespace Adriva.DevTools.CodeGenerator
{
    public interface ICodeBuilder : ISyntaxBuilder
    {
        ICodeBuilder WithNamespace(string namespaceName);

        ICodeBuilder AddUsingStatement(string namespaceName);

        ICodeBuilder AddClass(Action<IClassBuilder> buildClass);

        Task WriteAsync(TextWriter textWriter);
    }
}
