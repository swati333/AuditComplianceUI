using ActionPlan.Contracts.Requests;
using FluentValidation;

namespace ActionPlan.Application.Validators;

public sealed class AddActionPlanCommentRequestValidator : AbstractValidator<AddActionPlanCommentRequest>
{
    public AddActionPlanCommentRequestValidator()
    {
        RuleFor(x => x.AuthorId).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AuthorName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Text).NotEmpty().MaximumLength(4000);
    }
}
