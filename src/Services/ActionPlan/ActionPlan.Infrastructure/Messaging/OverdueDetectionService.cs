using ActionPlan.Application.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ActionPlan.Infrastructure.Messaging;

/// <summary>
/// Background escalation job for CLAUDE.md §2's "unclosed actions past due
/// date become Overdue and escalate." Polls every action plan still in an
/// owner-owned, not-yet-submitted state (see
/// ActionPlanStatusTransitions.CanBecomeOverdue) and flips any whose
/// DueDate has passed to Overdue via the aggregate's own MarkOverdue —
/// "escalate" here means raising ActionPlanOverdueDomainEvent, which the
/// DbContext turns into an outbox row (CLAUDE.md §8) for Notification
/// Service to eventually act on. Runs on its own DI scope per poll since
/// ActionPlanDbContext is scoped but this service is a singleton.
/// </summary>
public sealed class OverdueDetectionService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(15);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OverdueDetectionService> _logger;

    public OverdueDetectionService(IServiceScopeFactory scopeFactory, ILogger<OverdueDetectionService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DetectOverdueActionsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Unhandled error while detecting overdue action plans.");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown.
            }
        }
    }

    private async Task DetectOverdueActionsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IActionPlanRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var candidates = await repository.GetOverdueCandidatesAsync(cancellationToken);
        if (candidates.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var markedCount = 0;
        foreach (var actionPlan in candidates)
        {
            if (actionPlan.MarkOverdue(now))
            {
                markedCount++;
            }
        }

        if (markedCount > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Marked {Count} action plan(s) Overdue.", markedCount);
        }
    }
}
