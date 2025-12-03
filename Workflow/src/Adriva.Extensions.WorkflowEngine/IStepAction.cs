using System.Threading.Tasks;
using Adriva.Common.Core;

namespace Adriva.Extensions.WorkflowEngine
{
    public interface IStepAction
    {
        Task<object> RunAsync(WorkflowContext context, InputParameter[] inputParameters, DynamicItem properties);
    }
}
