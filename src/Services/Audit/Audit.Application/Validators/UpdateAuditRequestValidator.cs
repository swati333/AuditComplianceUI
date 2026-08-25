using Audit.Contracts.Requests;
using FluentValidation;

namespace Audit.Application.Validators;

public sealed class UpdateAuditRequestValidator : AbstractValidator<UpdateAuditRequest>
{
    public UpdateAuditRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Scope).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.RowVersion).NotEmpty().WithMessage("RowVersion is required for concurrency-safe updates.");
    }
}
