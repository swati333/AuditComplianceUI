using Audit.Contracts.Requests;
using FluentValidation;

namespace Audit.Application.Validators;

public sealed class CreateAuditRequestValidator : AbstractValidator<CreateAuditRequest>
{
    public CreateAuditRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Scope).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}
