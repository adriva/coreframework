using System;
using System.Collections.Generic;
using FluentValidation;

namespace Adriva.Extensions.WorkflowEngine.Validators
{
    internal sealed class WorkflowValidator : AbstractValidator<Workflow>
    {
        public WorkflowValidator()
        {
            this.RuleFor(x => x.Name).NotEmpty();
            this.RuleFor(x => x.Steps).NotEmpty();
            this.RuleForEach(x => x.Steps).SetValidator(new WorkflowStepValidator());
            this.RuleFor(x => x.Steps).Custom(this.ValidateUniqueNames);
        }

        private void ValidateUniqueNames(IEnumerable<WorkflowStep> steps, ValidationContext<Workflow> context)
        {
            if (null == steps)
            {
                return;
            }

            Queue<WorkflowStep> queue = new Queue<WorkflowStep>();
            HashSet<string> stepNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var step in steps)
            {
                queue.Enqueue(step);
            }

            while (0 < queue.Count)
            {
                var current = queue.Dequeue();

                if (stepNames.Contains(current.Name))
                {
                    context.AddFailure(nameof(WorkflowStep.Name), $"Workflow step name '{current.Name}' is not unique within the workflow instance.");
                    return;
                }
                else
                {
                    stepNames.Add(current.Name);
                    if (null != current.Steps)
                    {
                        foreach (var childStep in current.Steps)
                        {
                            queue.Enqueue(childStep);
                        }
                    }
                }
            }
        }
    }
}