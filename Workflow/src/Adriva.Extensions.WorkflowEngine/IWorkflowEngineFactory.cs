namespace Adriva.Extensions.WorkflowEngine
{
    public interface IWorkflowEngineFactory
    {
        IWorkflowEngine Get();

        IWorkflowEngine Get(string name);
    }
}
