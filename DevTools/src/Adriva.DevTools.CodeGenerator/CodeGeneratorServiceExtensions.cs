using Microsoft.Extensions.DependencyInjection;

namespace Adriva.DevTools.CodeGenerator
{
    public static class CodeGeneratorServiceExtensions
    {
        public static IServiceCollection AddCodeGenerators<TCodeBuilder, TClassBuilder, TPropertyBuilder>(this IServiceCollection services)
            where TCodeBuilder : class, ICodeBuilder
            where TClassBuilder : class, IClassBuilder
            where TPropertyBuilder : class, IPropertyBuilder
        {
            return services
                .AddTransient<ICodeBuilder, TCodeBuilder>()
                .AddTransient<IClassBuilder, TClassBuilder>()
                .AddTransient<IPropertyBuilder, TPropertyBuilder>()
                ;
        }

        public static IServiceCollection AddDefaultCSharpCodeGenerator(this IServiceCollection services)
        {
            return services.AddCodeGenerators<CSharpCodeBuilder, CSharpClassBuilder, CSharpPropertyBuilder>();
        }
    }
}
