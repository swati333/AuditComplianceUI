using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notification.Application.Common;

namespace Notification.Infrastructure.Email;

/// <summary>
/// Local-dev "email provider": never contacts a real mail server. Writes
/// each message as a timestamped, human-readable .eml-style file to
/// <see cref="FakeLocalEmailProviderOptions.OutputDirectory"/> (inspectable
/// like MailHog/Papercut/smtp4dev's local inbox) and logs a summary line.
/// CLAUDE.md §7 explicitly allows a documented local stand-in — swapping
/// this for a real provider (SendGrid, Graph, SMTP) is a single DI change;
/// nothing that calls <see cref="IEmailProvider"/> needs to change.
/// </summary>
public sealed class FakeLocalEmailProvider : IEmailProvider
{
    private readonly ILogger<FakeLocalEmailProvider> _logger;
    private readonly string _outputDirectory;

    public FakeLocalEmailProvider(IOptions<FakeLocalEmailProviderOptions> options, ILogger<FakeLocalEmailProvider> logger)
    {
        _logger = logger;
        _outputDirectory = options.Value.OutputDirectory;
    }

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_outputDirectory);

        var fileName = $"{DateTime.UtcNow:yyyyMMddTHHmmss.fffZ}_{Guid.NewGuid():N}.eml";
        var path = Path.Combine(_outputDirectory, fileName);

        var content = new StringBuilder()
            .AppendLine($"To: {message.ToUserId}")
            .AppendLine($"Subject: {message.Subject}")
            .AppendLine($"Date: {DateTime.UtcNow:R}")
            .AppendLine()
            .AppendLine(message.Body)
            .ToString();

        return WriteAndLogAsync(path, content, message, cancellationToken);
    }

    private async Task WriteAndLogAsync(string path, string content, EmailMessage message, CancellationToken cancellationToken)
    {
        await File.WriteAllTextAsync(path, content, cancellationToken);

        _logger.LogInformation(
            "Fake email delivered to {ToUserId} ({Subject}), written to {Path}.",
            message.ToUserId,
            message.Subject,
            path);
    }
}
