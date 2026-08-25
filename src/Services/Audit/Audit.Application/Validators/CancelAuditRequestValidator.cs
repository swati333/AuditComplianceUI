using Audit.Contracts.Requests;
using FluentValidation;

namespace Audit.Application.Validators;

public sealed class CancelAuditRequestValidator : AbstractValidator<CancelAuditRequest>
{
    public CancelAuditRequestValidator()
    {
        RuleFor(x => x.Reason).MaximumLength(1000);
    }
}
