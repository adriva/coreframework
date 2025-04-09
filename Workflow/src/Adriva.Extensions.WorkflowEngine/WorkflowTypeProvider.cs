using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core.CustomTypeProviders;

namespace Adriva.Extensions.WorkflowEngine
{
    internal sealed class WorkflowTypeProvider : DefaultDynamicLinqCustomTypeProvider
    {
        private readonly IEnumerable<Type> CustomTypes;

        public WorkflowTypeProvider(IEnumerable<Type> types) : base(System.Linq.Dynamic.Core.ParsingConfig.Default, true)
        {
            this.CustomTypes = types ?? Array.Empty<Type>();
        }

        public override HashSet<Type> GetCustomTypes()
        {
            return this.CustomTypes.ToHashSet();
        }
    }
}
