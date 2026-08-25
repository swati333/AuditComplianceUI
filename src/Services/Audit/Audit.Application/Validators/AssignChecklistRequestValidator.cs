using Audit.Contracts.Requests;
using FluentValidation;

namespace Audit.Application.Validators;

public sealed class AssignChecklistRequestValidator : AbstractValidator<AssignChecklistRequest>
{
    public AssignChecklistRequestValidator()
    {
        RuleFor(x => x.ChecklistId).NotEmpty();
    }
}
