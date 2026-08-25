using Finding.Contracts.Requests;
using FluentValidation;

namespace Finding.Application.Validators;

public sealed class AddDocumentRequestValidator : AbstractValidator<AddDocumentRequest>
{
    private const long MaxSizeBytes = 100 * 1024 * 1024; // 100 MB

    public AddDocumentRequestValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(260);
        RuleFor(x => x.BlobReference).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SizeBytes).GreaterThan(0).LessThanOrEqualTo(MaxSizeBytes);
    }
}
