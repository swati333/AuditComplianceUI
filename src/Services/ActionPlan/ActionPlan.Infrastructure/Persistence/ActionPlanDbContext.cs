using System.Text.Json;
using ContractsEvents = ActionPlan.Contracts.Events;
using ActionPlan.Domain.Entities;
using ActionPlan.Domain.Events;
using Ehs.Contracts.Events;
using Ehs.SharedKernel.Correlation;
using Ehs.SharedKernel.Domain;
using Ehs.SharedKernel.Messaging;
using Microsoft.EntityFrameworkCore;
using ActionPlanEntity = ActionPlan.Domain.Entities.ActionPlan;

namespace ActionPlan.Infrastructure.Persistence;

/// <summary>
/// Owns ActionPlanDb exclusively (CLAUDE.md §5). <see cref="SaveChangesAsync"/>
/// converts pending domain events on tracked entities into
/// <see cref="OutboxMessage"/> rows in the same local transaction as the
/// business change (CLAUDE.md §8) — the publish side of the Transactional
/// Outbox. <see cref="InboxMessages"/> is the consume side, written by
/// <c>Messaging.IntegrationEventConsumer</c> when handling Finding events.
/// </summary>
public sealed class ActionPlanDbContext : DbContext
{
    private readonly ICorrelationContextAccessor _correlationContextAccessor;

    public ActionPlanDbContext(DbContextOptions<ActionPlanDbContext> options, ICorrelationContextAccessor correlationContextAccessor)
        : base(options)
    {
        _correlationContextAccessor = correlationContextAccessor;
    }

    public DbSet<ActionPlanEntity> ActionPlans => Set<ActionPlanEntity>();

    public DbSet<FindingReference> FindingReferences => Set<FindingReference>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ActionPlanDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddOutboxMessagesFromDomainEvents();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void AddOutboxMessagesFromDomainEvents()
    {
        var entitiesWithEvents = ChangeTracker.Entries()
            .Select(e => e.Entity)
            .OfType<Entity<Guid>>()
            .Where(e => e.DomainEvents.Count > 0)
            .ToList();

        foreach (var entity in entitiesWithEvents)
        {
            foreach (var domainEvent in entity.DomainEvents)
            {
                OutboxMessages.Add(MapToOutboxMessage(domainEvent));
            }

            entity.ClearDomainEvents();
        }
    }

    /// <summary>
    /// 1:1 mapping from an in-process domain event to the wire-level
    /// integration event it represents. Add a case here whenever a new
    /// domain event is introduced that should cross the service boundary.
    /// </summary>
    private OutboxMessage MapToOutboxMessage(IDomainEvent domainEvent) => domainEvent switch
    {
        ActionPlanAssignedDomainEvent e => Build(new ContractsEvents.ActionPlanAssigned(e.ActionPlanId, e.FindingId, e.OwnerId, e.ApproverId, e.DueDate, e.Priority.ToString(), e.AssignedAtUtc)),
        ActionPlanStatusChangedDomainEvent e => Build(new ContractsEvents.ActionPlanStatusChanged(e.ActionPlanId, e.FindingId, e.FromStatus.ToString(), e.ToStatus.ToString(), e.ChangedAtUtc)),
        ActionPlanSubmittedDomainEvent e => Build(new ContractsEvents.ActionPlanSubmitted(e.ActionPlanId, e.FindingId, e.ApproverId, e.SubmittedAtUtc)),
        ActionPlanApprovedDomainEvent e => Build(new ContractsEvents.ActionPlanApproved(e.ActionPlanId, e.FindingId, e.ApprovedBy, e.ApprovedAtUtc)),
        ActionPlanRejectedDomainEvent e => Build(new ContractsEvents.ActionPlanRejected(e.ActionPlanId, e.FindingId, e.RejectedBy, e.Reason, e.RejectedAtUtc)),
        ActionPlanOverdueDomainEvent e => Build(new ContractsEvents.ActionPlanOverdue(e.ActionPlanId, e.FindingId, e.OwnerId, e.DueDate, e.DetectedAtUtc)),
        _ => throw new InvalidOperationException($"No outbox mapping registered for domain event '{domainEvent.GetType().Name}'."),
    };

    private OutboxMessage Build<TPayload>(TPayload payload)
        where TPayload : IIntegrationEvent
    {
        var correlationId = _correlationContextAccessor.CorrelationId == Guid.Empty
            ? Guid.NewGuid()
            : _correlationContextAccessor.CorrelationId;

        var envelope = EventEnvelope.Create(
            payload,
            source: "ActionPlanService",
            correlationId: correlationId,
            causationId: _correlationContextAccessor.CausationId);

        return new OutboxMessage
        {
            Id = envelope.EventId,
            Type = envelope.EventType,
            Content = JsonSerializer.Serialize(envelope),
            CorrelationId = envelope.CorrelationId,
            CausationId = envelope.CausationId,
            OccurredOnUtc = envelope.OccurredOnUtc,
        };
    }
}
