using Ehs.Contracts.Events;

namespace Audit.Contracts.Events;

public sealed record AuditPlanned(Guid AuditId, DateTime PlannedStartDate, DateTime PlannedEndDate) : IIntegrationEvent;
