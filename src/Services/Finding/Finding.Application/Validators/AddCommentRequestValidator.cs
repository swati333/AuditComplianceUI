using Finding.Contracts.Requests;
using FluentValidation;

namespace Finding.Application.Validators;

public sealed class AddCommentRequestValidator : AbstractValidator<AddCommentRequest>
{
    public AddCommentRequestValidator()
    {
        RuleFor(x => x.AuthorId).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AuthorName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Text).NotEmpty().MaximumLength(4000);
    }
}
