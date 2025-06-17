namespace Adriva.Extensions.WorkflowEngine
{
    public sealed class StepContext
    {
        internal StepCompilation StepCompilation { get; private set; }

        internal StepResult Result { get; private set; }

        public WorkflowStep Step { get; private set; }

        internal StepContext(StepCompilation stepCompilation)
        {
            this.StepCompilation = stepCompilation;
            this.Result = new StepResult(stepCompilation.Step);
            this.Step = stepCompilation.Step.Clone(false);
        }

    }
}
