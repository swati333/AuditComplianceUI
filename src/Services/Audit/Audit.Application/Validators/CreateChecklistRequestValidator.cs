using Audit.Contracts.Requests;
using FluentValidation;

namespace Audit.Application.Validators;

public sealed class CreateChecklistRequestValidator : AbstractValidator<CreateChecklistRequest>
{
    public CreateChecklistRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleForEach(x => x.Questions).SetValidator(new CreateChecklistQuestionRequestValidator());
    }
}

public sealed class CreateChecklistQuestionRequestValidator : AbstractValidator<CreateChecklistQuestionRequest>
{
    public CreateChecklistQuestionRequestValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
