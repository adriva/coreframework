using Microsoft.Extensions.DependencyInjection;

namespace Adriva.DevTools.CodeGenerator
{
    public static class CodeGeneratorServiceExtensions
    {
        public static IServiceCollection AddCodeGenerators<TCodeBuilder, TClassBuilder, TPropertyBuilder>(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
            where TCodeBuilder : class, ICodeBuilder
            where TClassBuilder : class, IClassBuilder
            where TPropertyBuilder : class, IPropertyBuilder
        {
            services.Add(ServiceDescriptor.Describe(typeof(ICodeBuilder), typeof(TCodeBuilder), serviceLifetime));
            services.Add(ServiceDescriptor.Describe(typeof(IClassBuilder), typeof(TClassBuilder), serviceLifetime));
            services.Add(ServiceDescriptor.Describe(typeof(IPropertyBuilder), typeof(TPropertyBuilder), serviceLifetime));

            return services;
        }

        public static IServiceCollection AddCSharpCodeGenerator(this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        {
            return services.AddCodeGenerators<CSharpCodeBuilder, CSharpClassBuilder, CSharpPropertyBuilder>(serviceLifetime);
        }
    }
}
