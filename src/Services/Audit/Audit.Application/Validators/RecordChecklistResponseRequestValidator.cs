using Audit.Contracts.Requests;
using FluentValidation;

namespace Audit.Application.Validators;

public sealed class RecordChecklistResponseRequestValidator : AbstractValidator<RecordChecklistResponseRequest>
{
    public RecordChecklistResponseRequestValidator()
    {
        RuleFor(x => x.ChecklistQuestionId).NotEmpty();
        RuleFor(x => x.AnswerText).NotEmpty().MaximumLength(4000);
    }
}
