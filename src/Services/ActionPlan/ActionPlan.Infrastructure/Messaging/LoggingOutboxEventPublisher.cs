using Ehs.SharedKernel.Messaging;
using Microsoft.Extensions.Logging;

namespace ActionPlan.Infrastructure.Messaging;

/// <summary>
/// Local-dev/demo transport: logs the fully-formed event envelope instead of
/// sending it to a real broker (CLAUDE.md §7 explicitly allows this for
/// local dev). Swapping for Azure Service Bus later is a single DI change.
/// </summary>
public sealed class LoggingOutboxEventPublisher : IOutboxEventPublisher
{
    private readonly ILogger<LoggingOutboxEventPublisher> _logger;

    public LoggingOutboxEventPublisher(ILogger<LoggingOutboxEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Publishing integration event {EventType} (OutboxMessageId={OutboxMessageId}, CorrelationId={CorrelationId}): {Content}",
            message.Type,
            message.Id,
            message.CorrelationId,
            message.Content);
        return Task.CompletedTask;
    }
}
