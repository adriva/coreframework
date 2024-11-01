using System.Threading.Tasks;

namespace Adriva.Extensions.WorkflowEngine
{
    public interface IWorkflowEngine
    {
        Task<WorkflowResults> RunAsync(string json, params InputParameter[] inputParameters);

        Task<WorkflowResults> RunAsync(Workflow workflow, params InputParameter[] inputParameters);
    }
}
