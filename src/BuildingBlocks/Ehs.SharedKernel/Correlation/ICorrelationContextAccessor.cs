namespace Ehs.SharedKernel.Correlation;

/// <summary>
/// Ambient accessor for the current request/message's correlation and
/// causation IDs. Application/Infrastructure code (e.g. outbox writers,
/// event publishers) depends on this abstraction, not on ASP.NET Core;
/// Ehs.Observability supplies the HTTP-request-scoped implementation.
/// </summary>
public interface ICorrelationContextAccessor
{
    Guid CorrelationId { get; }

    Guid? CausationId { get; }
}
