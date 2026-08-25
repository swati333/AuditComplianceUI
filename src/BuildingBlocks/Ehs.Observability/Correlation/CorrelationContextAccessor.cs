using Ehs.SharedKernel.Correlation;

namespace Ehs.Observability.Correlation;

/// <summary>
/// AsyncLocal-backed implementation of <see cref="ICorrelationContextAccessor"/>,
/// scoped to the logical async flow of one request (or one message-consumer
/// invocation). <see cref="Set"/> is called once by <see cref="CorrelationMiddleware"/>.
/// </summary>
public sealed class CorrelationContextAccessor : ICorrelationContextAccessor
{
    private static readonly AsyncLocal<CorrelationContext?> Current = new();

    public Guid CorrelationId => Current.Value?.CorrelationId ?? Guid.Empty;

    public Guid? CausationId => Current.Value?.CausationId;

    public void Set(Guid correlationId, Guid? causationId) =>
        Current.Value = new CorrelationContext(correlationId, causationId);

    private sealed record CorrelationContext(Guid CorrelationId, Guid? CausationId);
}
