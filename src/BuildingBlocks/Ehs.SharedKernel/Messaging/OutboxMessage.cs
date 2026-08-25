namespace Ehs.SharedKernel.Messaging;

/// <summary>
/// Transactional outbox row. Written in the same local DB transaction as the
/// business change it originated from (CLAUDE.md §8); a background worker in
/// each service's Infrastructure layer polls unprocessed rows and publishes
/// them to Service Bus, marking them sent or recording the failure.
/// This is a plain persistence-ignorant POCO — each service maps its own
/// EF Core configuration/table for it; no shared database is implied.
/// </summary>
public class OutboxMessage
{
    public Guid Id { get; set; }

    public string Type { get; set; } = default!;

    public string Content { get; set; } = default!;

    public Guid CorrelationId { get; set; }

    public Guid? CausationId { get; set; }

    public DateTime OccurredOnUtc { get; set; }

    public DateTime? ProcessedOnUtc { get; set; }

    public string? Error { get; set; }

    public int RetryCount { get; set; }
}
