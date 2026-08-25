using Audit.Contracts.Requests;
using FluentValidation;

namespace Audit.Application.Validators;

public sealed class PlanAuditRequestValidator : AbstractValidator<PlanAuditRequest>
{
    public PlanAuditRequestValidator()
    {
        RuleFor(x => x.PlannedEndDate)
            .GreaterThanOrEqualTo(x => x.PlannedStartDate)
            .WithMessage("Planned end date must be on or after the planned start date.");
    }
}
