using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Forms;

public interface IDynamicFormsBuilder
{
    IServiceCollection Services { get; }

    IDynamicFormsBuilder UseRepository<T>() where T : class, IDynamicFormsRepository;
}
