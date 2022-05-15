using System.Threading.Tasks;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class PostProcessor
    {
        public virtual void PostProcess(DataSet dataSet)
        {

        }

        public virtual Task PostProcessAsync(DataSet dataSet)
        {
            this.PostProcess(dataSet);
            return Task.CompletedTask;
        }
    }
}