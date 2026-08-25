using System.Text.Json;
using Audit.Domain.Entities;
using Audit.Domain.Events;
using ContractsEvents = Audit.Contracts.Events;
using Ehs.Contracts.Events;
using Ehs.SharedKernel.Correlation;
using Ehs.SharedKernel.Domain;
using Ehs.SharedKernel.Messaging;
using Microsoft.EntityFrameworkCore;
using AuditEntity = Audit.Domain.Entities.Audit;

namespace Audit.Infrastructure.Persistence;

/// <summary>
/// Owns AuditDb exclusively (CLAUDE.md §5). <see cref="SaveChangesAsync"/> is
/// overridden to convert every pending domain event on tracked entities into
/// an <see cref="OutboxMessage"/> row and add it to the same unit of work,
/// so the business change and the outbox write commit in one local
/// transaction (CLAUDE.md §8) — this is the entire "Transactional Outbox"
/// implementation; nothing about the write side is spread elsewhere.
/// </summary>
public sealed class AuditDbContext : DbContext
{
    private readonly ICorrelationContextAccessor _correlationContextAccessor;

    public AuditDbContext(DbContextOptions<AuditDbContext> options, ICorrelationContextAccessor correlationContextAccessor)
        : base(options)
    {
        _correlationContextAccessor = correlationContextAccessor;
    }

    public DbSet<AuditEntity> Audits => Set<AuditEntity>();

    public DbSet<Checklist> Checklists => Set<Checklist>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditDbContext).Assembly);
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
        AuditCreatedDomainEvent e => Build(new ContractsEvents.AuditCreated(e.AuditId, e.Title, e.Scope, e.Location, e.CreatedAtUtc)),
        AuditPlannedDomainEvent e => Build(new ContractsEvents.AuditPlanned(e.AuditId, e.PlannedStartDate, e.PlannedEndDate)),
        AuditStartedDomainEvent e => Build(new ContractsEvents.AuditStarted(e.AuditId, e.ActualStartDateUtc)),
        AuditCompletedDomainEvent e => Build(new ContractsEvents.AuditCompleted(e.AuditId, e.ActualEndDateUtc)),
        AuditClosedDomainEvent e => Build(new ContractsEvents.AuditClosed(e.AuditId, e.ClosedAtUtc)),
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
            source: "AuditService",
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
