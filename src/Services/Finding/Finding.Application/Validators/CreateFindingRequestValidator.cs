using Finding.Contracts.Requests;
using Finding.Domain.Enums;
using FluentValidation;

namespace Finding.Application.Validators;

public sealed class CreateFindingRequestValidator : AbstractValidator<CreateFindingRequest>
{
    public CreateFindingRequestValidator()
    {
        RuleFor(x => x.AuditId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.Severity)
            .NotEmpty()
            .Must(severity => Enum.TryParse<FindingSeverity>(severity, ignoreCase: true, out _))
            .WithMessage($"Severity must be one of: {string.Join(", ", Enum.GetNames<FindingSeverity>())}.");
    }
}
