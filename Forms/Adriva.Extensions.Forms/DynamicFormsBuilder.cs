using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Adriva.Extensions.Forms;

internal sealed class DynamicFormsBuilder(IServiceCollection services) : IDynamicFormsBuilder
{
    public IServiceCollection Services { get; } = services;

    public IDynamicFormsBuilder UseRepository<T>() where T : class, IDynamicFormsRepository
    {
        this.Services.TryAddEnumerable(ServiceDescriptor.Scoped<IDynamicFormsRepository, T>());
        return this;
    }
}
