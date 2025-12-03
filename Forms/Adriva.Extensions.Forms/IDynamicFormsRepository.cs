namespace Adriva.Extensions.Forms;

public interface IDynamicFormsRepository
{
    Task<DynamicForm> LoadAsync(string formName);

    Task<object> SaveAsync(DynamicForm form);
}