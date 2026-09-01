using ActionPlan.Contracts.Events;
using Audit.Application.Common;
using Ehs.Contracts.Events;
using Ehs.EventBus;

namespace Audit.Application.EventHandlers;

/// <summary>
/// Closes the local <see cref="Audit.Domain.Entities.OpenRequiredAction"/>
/// projection entry once the action plan reaches Closed or Cancelled — the
/// only two <c>ToStatus</c> values ActionPlan.Domain.Entities.ActionPlan
/// ever raises ActionPlanStatusChanged for besides InProgress (which doesn't
/// affect openness). Approved/Rejected/Submitted/Overdue leave the action
/// open, matching CLAUDE.md §2 ("unclosed actions ... become Overdue").
/// </summary>
public sealed class ActionPlanStatusChangedHandler : IIntegrationEventHandler<ActionPlanStatusChanged>
{
    private static readonly string[] TerminalStatuses = ["Closed", "Cancelled"];

    private readonly IOpenRequiredActionRepository _openRequiredActionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActionPlanStatusChangedHandler(IOpenRequiredActionRepository openRequiredActionRepository, IUnitOfWork unitOfWork)
    {
        _openRequiredActionRepository = openRequiredActionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(EventEnvelope<ActionPlanStatusChanged> envelope, CancellationToken cancellationToken = default)
    {
        if (!TerminalStatuses.Contains(envelope.Payload.ToStatus, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        var entry = await _openRequiredActionRepository.GetByActionPlanIdAsync(envelope.Payload.ActionPlanId, cancellationToken);
        if (entry is null)
        {
            // Wasn't tracked (Low/Medium finding, or ActionPlanAssigned not yet
            // consumed) — nothing local to close.
            return;
        }

        _openRequiredActionRepository.Remove(entry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
