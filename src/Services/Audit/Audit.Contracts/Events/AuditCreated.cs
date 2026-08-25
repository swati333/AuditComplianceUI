using Ehs.Contracts.Events;

namespace Audit.Contracts.Events;

public sealed record AuditCreated(Guid AuditId, string Title, string Scope, string Location, DateTime CreatedAtUtc) : IIntegrationEvent;
