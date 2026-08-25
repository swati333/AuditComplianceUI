using ActionPlan.Contracts.Requests;
using ActionPlan.Domain.Enums;
using FluentValidation;

namespace ActionPlan.Application.Validators;

public sealed class UpdateActionPlanRequestValidator : AbstractValidator<UpdateActionPlanRequest>
{
    public UpdateActionPlanRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.OwnerId).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OwnerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ApproverId).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ApproverName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DueDate).NotEmpty();
        RuleFor(x => x.ActionType)
            .NotEmpty()
            .Must(value => Enum.TryParse<ActionType>(value, ignoreCase: true, out _))
            .WithMessage($"ActionType must be one of: {string.Join(", ", Enum.GetNames<ActionType>())}.");
        RuleFor(x => x.Priority)
            .NotEmpty()
            .Must(value => Enum.TryParse<ActionPriority>(value, ignoreCase: true, out _))
            .WithMessage($"Priority must be one of: {string.Join(", ", Enum.GetNames<ActionPriority>())}.");
        RuleFor(x => x)
            .Must(x => !string.Equals(x.OwnerId, x.ApproverId, StringComparison.OrdinalIgnoreCase))
            .WithName("approverId")
            .WithMessage("The approver cannot be the same person as the owner (self-approval is not allowed).");
        RuleFor(x => x.RowVersion).NotEmpty().WithMessage("RowVersion is required for concurrency-safe updates.");
    }
}
