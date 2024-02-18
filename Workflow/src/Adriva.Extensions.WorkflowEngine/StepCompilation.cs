using System;
using System.Collections.Generic;

namespace Adriva.Extensions.WorkflowEngine
{
    internal sealed class StepCompilation
    {
        public WorkflowStep Step { get; private set; }

        public Func<object, bool> Predicate { get; private set; }

        public Type ActionType { get; private set; }

        public IList<StepCompilation> Children { get; private set; }

        public StepCompilation(WorkflowStep step, Func<object, bool> predicate, Type actionType)
        {
            this.Step = step;
            this.Predicate = predicate;
            this.ActionType = actionType;
            this.Children = new List<StepCompilation>();

            if (null == predicate && null == this.ActionType)
            {
                throw new InvalidOperationException($"A workflow step must have at least one predicate or action defined. '{step.Name}'");
            }
        }
    }
}
