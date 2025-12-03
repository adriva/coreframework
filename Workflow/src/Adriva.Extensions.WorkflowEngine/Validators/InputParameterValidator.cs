using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace Adriva.Extensions.WorkflowEngine.Validators
{
    internal sealed class InputParametersValidator : AbstractValidator<IEnumerable<InputParameter>>
    {
        public InputParametersValidator()
        {
            this.RuleFor(x =>
                            x
                                .Select(y => y.Name)
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .Count())
                            .Must((allParameters, distinctCount) => distinctCount == allParameters.Count())
                            .WithMessage("Input parameter names should be unique.");
            this.RuleForEach(x => x).SetValidator(new InputParameterValidator());
        }
    }

    internal sealed class InputParameterValidator : AbstractValidator<InputParameter>
    {
        public InputParameterValidator()
        {
            this.RuleFor(x => x.Name).NotEmpty().Matches(@"^([a-z]|[A-Z]|_)+\w*$").WithMessage(p => $"Input parameter is empty or doesn't match C# property name specifications.{p.Name}");
        }
    }
}