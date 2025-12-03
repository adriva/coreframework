using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Adriva.Extensions.Forms;

public class DynamicFormManager(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
{
    private readonly IServiceProvider ServiceProvider = serviceProvider;
    private readonly ILogger Logger = loggerFactory.CreateLogger<DynamicFormManager>();

    public Task<DynamicForm> LoadAsync(string formName)
    {
        var firstRepository = this.ServiceProvider.GetServices<IDynamicFormsRepository>().FirstOrDefault();
        return this.LoadAsync(formName, firstRepository);
    }

    public Task<object> SaveAsync(DynamicForm? form)
    {
        var firstRepository = this.ServiceProvider.GetServices<IDynamicFormsRepository>().FirstOrDefault();
        return this.SaveAsync(form, firstRepository);
    }

    public Task<DynamicForm> LoadAsync(string formName, IDynamicFormsRepository repository)
    {
        if (string.IsNullOrWhiteSpace(formName))
        {
            throw new ArgumentNullException(nameof(formName));
        }

        if (repository is null)
        {
            throw new ArgumentNullException(nameof(repository));
        }

        this.Logger.LogTrace("Using repository {repo} to load form '{name}'.", repository.GetType().FullName, formName);

        try
        {
            return repository.LoadAsync(formName);
        }
        catch (Exception fatalError)
        {
            this.Logger.LogError(fatalError, "Error loading form '{name}' from repository '{repo}'.", formName, repository.GetType().FullName);
            throw;
        }
    }

    public Task<object> SaveAsync(DynamicForm? form, IDynamicFormsRepository repository)
    {
        if (form is null)
        {
            throw new ArgumentNullException(nameof(form), "Form is not set to an instance of an object.");
        }
        else if (string.IsNullOrWhiteSpace(form.Name))
        {
            throw new ArgumentNullException(nameof(form.Name), "Form name must be set.");
        }

        this.Logger.LogTrace("Using repository {repo} to save form '{name}'.", repository.GetType().FullName, form.Name);

        try
        {
            return repository.SaveAsync(form);
        }
        catch (Exception fatalError)
        {
            this.Logger.LogError(fatalError, "Error saving '{name}' to repository '{repo}'.", form.Name, repository.GetType().FullName);
            throw;
        }
    }
}
