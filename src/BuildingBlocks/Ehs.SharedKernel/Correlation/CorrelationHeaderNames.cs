namespace Ehs.SharedKernel.Correlation;

/// <summary>
/// HTTP header names used to propagate correlation/causation IDs across
/// service boundaries (CLAUDE.md §7). Kept dependency-free so it can be
/// referenced from HTTP clients, middleware and event publishers alike.
/// </summary>
public static class CorrelationHeaderNames
{
    public const string CorrelationId = "X-Correlation-Id";

    public const string CausationId = "X-Causation-Id";
}
