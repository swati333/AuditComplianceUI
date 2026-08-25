namespace Notification.Application.Common;

/// <summary>
/// Email-provider abstraction. <see cref="EmailMessage.ToUserId"/> is a
/// UserId, not an address — Notification Service doesn't own user records
/// (no Identity/Users integration exists yet), so translating UserId to an
/// actual mailbox address is the provider's job. The local
/// <c>FakeLocalEmailProvider</c> (Notification.Infrastructure) fabricates an
/// address for display purposes only; a real provider (SendGrid, Graph, SMTP)
/// would resolve it via Entra ID / a Users service.
/// </summary>
public interface IEmailProvider
{
    /// <summary>Throws on failure — the caller (NotificationDispatcher) is responsible for retry/dead-letter bookkeeping.</summary>
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}

public sealed record EmailMessage(string ToUserId, string Subject, string Body);
