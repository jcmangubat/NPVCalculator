using FluentValidation;
using NPVCalculator.Domain.Models;

namespace NPVCalculator.Application.Validators;

public class NpvRequestValidator : AbstractValidator<NpvRequest>
{
    public NpvRequestValidator()
    {
        RuleFor(x => x.CashFlows)
            .NotEmpty().WithMessage("CashFlows are required.")
            .Must(cf => cf.Count <= 100).WithMessage("Maximum of 100 cash flows are allowed.");

        RuleFor(x => x.LowerBoundRate)
            .GreaterThanOrEqualTo(0).WithMessage("LowerBoundRate must be at least 0.");

        RuleFor(x => x.UpperBoundRate)
            .GreaterThanOrEqualTo(x => x.LowerBoundRate)
            .WithMessage("UpperBoundRate must be greater than or equal to LowerBoundRate.");

        RuleFor(x => x.Increment)
            .GreaterThan(0).WithMessage("Increment must be greater than 0.");
    }
}