using Adriva.Extensions.Forms;

namespace Microsoft.Extensions.DependencyInjection;

public static class FormsServiceCollectionExtensions
{
    public static IDynamicFormsBuilder AddDynamicForms(this IServiceCollection services)
    {
        services.AddScoped<DynamicFormManager>();

        return new DynamicFormsBuilder(services);
    }
}
