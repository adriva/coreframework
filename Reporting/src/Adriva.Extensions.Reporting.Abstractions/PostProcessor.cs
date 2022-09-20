using System.Threading.Tasks;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class PostProcessor
    {
        public virtual void PostProcess(DataSet dataSet, ReportCommand command)
        {

        }

        public virtual Task PostProcessAsync(DataSet dataSet, ReportCommand command)
        {
            this.PostProcess(dataSet, command);
            return Task.CompletedTask;
        }
    }
}