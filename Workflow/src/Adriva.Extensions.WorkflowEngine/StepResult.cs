using System;
using System.Collections.Generic;

namespace Adriva.Extensions.WorkflowEngine
{
    public sealed class StepResult
    {
        private readonly List<StepResult> ChildResultItems = new List<StepResult>();

        public IReadOnlyList<StepResult> ChildResults => this.ChildResultItems.AsReadOnly();

        public bool PredicateResult { get; internal set; }

        public object ActionResult { get; internal set; }

        public Exception Exception { get; internal set; }

        public WorkflowStep Step { get; private set; }

        internal void AddChildResult(StepResult stepResult)
        {
            if (null != stepResult)
            {
                this.ChildResultItems.Add(stepResult);
            }
        }

        internal StepResult()
        {
            this.Step = null;
        }

        public StepResult(WorkflowStep step)
        {
            this.Step = step?.Clone(false);
        }
    }
}
