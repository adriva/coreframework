using System.Collections.Generic;
using Adriva.Common.Core;
using Newtonsoft.Json;

namespace Adriva.Extensions.WorkflowEngine
{
    public class WorkflowStep
    {
        public string Name { get; set; }

        public IDictionary<string, object> Properties { get; set; }

        public string Predicate { get; set; }

        public bool IsEnabled { get; set; }

        public ActionOperator Operator { get; set; }

        public StepAction Action { get; set; }

        public IEnumerable<WorkflowStep> Steps { get; set; }

        [JsonProperty("tags", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string[] Tags { get; set; }

        public WorkflowStep Clone(bool includeChildren)
        {
            var clone = new WorkflowStep()
            {
                Name = this.Name,
                Predicate = this.Predicate,
                IsEnabled = this.IsEnabled,
                Operator = this.Operator,
                Action = this.Action,
                Properties = this.Properties,
                Tags = this.Tags
            };

            if (includeChildren && null != this.Steps)
            {
                var tempSteps = new List<WorkflowStep>();
                this.Steps.ForEach((index, childStep) =>
                {
                    tempSteps.Add(childStep.Clone(includeChildren));
                });
                clone.Steps = tempSteps.AsReadOnly();
            }

            return clone;
        }
    }
}
