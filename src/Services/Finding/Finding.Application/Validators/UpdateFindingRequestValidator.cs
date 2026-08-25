using Finding.Contracts.Requests;
using Finding.Domain.Enums;
using FluentValidation;

namespace Finding.Application.Validators;

public sealed class UpdateFindingRequestValidator : AbstractValidator<UpdateFindingRequest>
{
    public UpdateFindingRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.Severity)
            .NotEmpty()
            .Must(severity => Enum.TryParse<FindingSeverity>(severity, ignoreCase: true, out _))
            .WithMessage($"Severity must be one of: {string.Join(", ", Enum.GetNames<FindingSeverity>())}.");
        RuleFor(x => x.RowVersion).NotEmpty().WithMessage("RowVersion is required for concurrency-safe updates.");
    }
}
