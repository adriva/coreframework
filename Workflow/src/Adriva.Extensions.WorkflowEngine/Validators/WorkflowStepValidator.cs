using FluentValidation;

namespace Adriva.Extensions.WorkflowEngine.Validators
{
    internal sealed class WorkflowStepValidator : AbstractValidator<WorkflowStep>
    {
        public WorkflowStepValidator()
        {
            this.RuleFor(x => x.Name).NotEmpty();
            this.RuleFor(x => x.Predicate).NotEmpty().When(x => null == x.Action);
            this.RuleFor(x => x.Action).NotNull().When(x => string.IsNullOrWhiteSpace(x.Predicate));
        }
    }
}