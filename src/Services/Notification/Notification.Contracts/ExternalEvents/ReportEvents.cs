using Ehs.Contracts.Events;

namespace Notification.Contracts.ExternalEvents;

/// <summary>
/// Reporting Service (CLAUDE.md phase 5) does not exist yet — see
/// ActionPlanEvents.cs's doc comment for the full rationale on why this
/// local placeholder contract exists instead of a reference to a real
/// Reporting.Contracts project.
/// </summary>
public sealed record ReportGenerated(Guid ReportId, string ReportType, string RequestedByUserId, string BlobReference, DateTime GeneratedAtUtc) : IIntegrationEvent;

public sealed record ReportGenerationFailed(Guid ReportId, string ReportType, string RequestedByUserId, string ErrorMessage, DateTime FailedAtUtc) : IIntegrationEvent;
