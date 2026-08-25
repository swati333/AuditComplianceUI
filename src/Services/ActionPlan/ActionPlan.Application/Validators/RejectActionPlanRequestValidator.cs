using ActionPlan.Contracts.Requests;
using FluentValidation;

namespace ActionPlan.Application.Validators;

/// <summary>Enforces the mandatory rejection reason (CLAUDE.md — "Reject with mandatory reason").</summary>
public sealed class RejectActionPlanRequestValidator : AbstractValidator<RejectActionPlanRequest>
{
    public RejectActionPlanRequestValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().WithMessage("A rejection reason is required.").MaximumLength(2000);
    }
}
