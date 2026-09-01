using System.Text.Json;
using ContractsEvents = Finding.Contracts.Events;
using Ehs.Contracts.Events;
using Ehs.SharedKernel.Correlation;
using Ehs.SharedKernel.Domain;
using Ehs.SharedKernel.Messaging;
using Finding.Domain.Entities;
using Finding.Domain.Events;
using Microsoft.EntityFrameworkCore;
using FindingEntity = Finding.Domain.Entities.Finding;

namespace Finding.Infrastructure.Persistence;

/// <summary>
/// Owns FindingDb exclusively (CLAUDE.md §5). <see cref="SaveChangesAsync"/>
/// converts pending domain events on tracked entities into
/// <see cref="OutboxMessage"/> rows in the same local transaction as the
/// business change (CLAUDE.md §8) — the publish side of the Transactional
/// Outbox. <see cref="InboxMessages"/> is the consume side, written by
/// <c>Messaging.IntegrationEventConsumer</c> when handling Audit events.
/// </summary>
public sealed class FindingDbContext : DbContext
{
    private readonly ICorrelationContextAccessor _correlationContextAccessor;

    public FindingDbContext(DbContextOptions<FindingDbContext> options, ICorrelationContextAccessor correlationContextAccessor)
        : base(options)
    {
        _correlationContextAccessor = correlationContextAccessor;
    }

    public DbSet<FindingEntity> Findings => Set<FindingEntity>();

    public DbSet<AuditReference> AuditReferences => Set<AuditReference>();

    public DbSet<ActionPlanReference> ActionPlanReferences => Set<ActionPlanReference>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FindingDbContext).Assembly);
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
        FindingCreatedDomainEvent e => Build(new ContractsEvents.FindingCreated(e.FindingId, e.AuditId, e.Title, e.Severity.ToString(), e.CreatedAtUtc)),
        CriticalFindingCreatedDomainEvent e => Build(new ContractsEvents.CriticalFindingCreated(e.FindingId, e.AuditId, e.Title, e.CreatedAtUtc)),
        FindingResolvedDomainEvent e => Build(new ContractsEvents.FindingResolved(e.FindingId, e.AuditId, e.ResolvedAtUtc)),
        FindingClosedDomainEvent e => Build(new ContractsEvents.FindingClosed(e.FindingId, e.AuditId, e.ClosedAtUtc)),
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
            source: "FindingService",
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
