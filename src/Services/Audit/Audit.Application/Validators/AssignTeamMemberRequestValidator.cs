using Audit.Contracts.Requests;
using Audit.Domain.Enums;
using FluentValidation;

namespace Audit.Application.Validators;

public sealed class AssignTeamMemberRequestValidator : AbstractValidator<AssignTeamMemberRequest>
{
    public AssignTeamMemberRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(role => Enum.TryParse<AuditTeamRole>(role, ignoreCase: true, out _))
            .WithMessage($"Role must be one of: {nameof(AuditTeamRole.Auditor)}, {nameof(AuditTeamRole.Auditee)}.");
    }
}
