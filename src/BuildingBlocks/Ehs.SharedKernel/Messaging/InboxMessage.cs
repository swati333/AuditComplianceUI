namespace Ehs.SharedKernel.Messaging;

/// <summary>
/// Processed-message record for idempotent consumption (CLAUDE.md §8). The
/// dedupe key is (Id, ConsumerName): the same event delivered twice to the
/// same handler must not repeat its side effect, but distinct handlers in the
/// same service each get their own row for the same incoming event.
/// </summary>
public class InboxMessage
{
    public Guid Id { get; set; }

    public string ConsumerName { get; set; } = default!;

    public string Type { get; set; } = default!;

    public DateTime ProcessedOnUtc { get; set; }
}
