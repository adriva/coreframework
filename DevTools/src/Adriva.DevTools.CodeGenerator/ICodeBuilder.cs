using System;
using System.IO;

namespace Adriva.DevTools.CodeGenerator
{
    public interface ICodeBuilder : ISyntaxBuilder
    {
        ICodeBuilder WithNamespace(string namespaceName);

        ICodeBuilder AddUsingStatement(string namespaceName);

        ICodeBuilder AddClass(Action<IClassBuilder> buildClass);

        void WriteTo(TextWriter textWriter);
    }
}
