using System.Collections.Generic;
using Adriva.Common.Core;

namespace Adriva.Extensions.WorkflowEngine
{
    public class Workflow
    {
        public string Name { get; set; }

        public IEnumerable<WorkflowStep> Steps { get; set; }

        public Workflow Clone()
        {
            var clone = new Workflow()
            {
                Name = this.Name
            };

            if (null != this.Steps)
            {
                var tempSteps = new List<WorkflowStep>();
                this.Steps.ForEach((index, childStep) =>
                {
                    tempSteps.Add(childStep.Clone(true));
                });
                clone.Steps = tempSteps.AsReadOnly();
            }

            return clone;
        }
    }
}
