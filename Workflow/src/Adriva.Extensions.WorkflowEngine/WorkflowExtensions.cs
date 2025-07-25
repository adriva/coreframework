using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Adriva.Common.Core;

namespace Adriva.Extensions.WorkflowEngine
{
    public static class WorkflowExtensions
    {
        public static string CalculateHash(this Workflow workflow)
        {
            if (null == workflow)
            {
                return string.Empty;
            }

            if (null == workflow.Steps)
            {
                return Utilities.CalculateHash(workflow.Name);
            }

            StringBuilder buffer = new StringBuilder();

            foreach (var step in workflow.Steps)
            {
                buffer.Append($"{step.CalculateHash()}:");
            }

            return Utilities.CalculateHash(buffer.ToString());
        }

        private static string CalculateHash(this WorkflowStep workflowStep)
        {
            if (null == workflowStep)
            {
                return string.Empty;
            }

            StringBuilder buffer = new StringBuilder();

            Queue<WorkflowStep> queue = new Queue<WorkflowStep>();
            queue.Enqueue(workflowStep);

            while (queue.TryDequeue(out WorkflowStep currentStep))
            {
                buffer.Append($"{currentStep.Name}:{currentStep.Predicate}:{currentStep.Operator}");
                buffer.Append($":{currentStep.Action?.Target}:{currentStep.Action?.Name}");
                buffer.Append($":{currentStep.Operator}");
                buffer.Append($":{currentStep.IsEnabled}");

                if (null != currentStep.Properties)
                {
                    currentStep.Properties.ForEach((index, pair) =>
                    {
                        buffer.Append($":{pair.Key}:{Utilities.SafeSerialize(pair.Value)}");
                    });
                }

                if (null != currentStep.Steps)
                {
                    foreach (var childStep in currentStep.Steps)
                    {
                        buffer.Append($":{childStep.CalculateHash()}");
                    }
                }
            }

            return buffer.ToString();
        }

        public static bool TryGetInputValue<T>(this InputParameter[] inputParameters, string name, out T value)
        {
            value = default;

            if (null == inputParameters || string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            var inputParameter = inputParameters.FirstOrDefault(x => 0 == string.Compare(x.Name, name, StringComparison.OrdinalIgnoreCase));

            if (null == inputParameter)
            {
                return false;
            }

            try
            {
                value = (T)inputParameter.Value;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
