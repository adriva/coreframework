using System;
using System.Collections;
using System.Collections.Generic;
using Adriva.Common.Core;

namespace Adriva.Extensions.WorkflowEngine
{
    public sealed class WorkflowResults : IEnumerable<StepResult>, IDisposable
    {
        private readonly IList<StepResult> StepResults;

        public bool HasError { get; internal set; }

        internal WorkflowResults(IList<StepResult> stepResults)
        {
            this.StepResults = stepResults ?? Array.Empty<StepResult>();
        }

        public StepResult this[int index]
        {
            get => this.StepResults[index];
        }

        public IEnumerator<StepResult> GetEnumerator()
        {
            return this.StepResults.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.StepResults).GetEnumerator();
        }

        public void Dispose()
        {
            if (0 < this.StepResults.Count)
            {
                this.StepResults.ForEach((index, stepResult) =>
                {
                    if (stepResult.ActionResult is IDisposable disposableActionResult)
                    {
                        disposableActionResult.Dispose();
                    }
                });
            }
        }
    }
}
