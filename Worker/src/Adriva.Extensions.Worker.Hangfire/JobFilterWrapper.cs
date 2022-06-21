using System;
using Hangfire.Common;

namespace Adriva.Extensions.Worker.Hangfire
{
    internal sealed class JobFilterWrapper : JobFilter
    {
        public JobFilterWrapper(IServiceProvider serviceProvider, object instance, JobFilterScope scope, int? order)
            : base(new JobFilterAttributeWrapper(serviceProvider, instance), scope, order)
        {

        }
    }
}
