namespace Audit.Contracts.Requests;

public sealed record PlanAuditRequest(DateTime PlannedStartDate, DateTime PlannedEndDate);
