using System.Text.RegularExpressions;
using Ehs.SharedKernel.Domain;
using Ehs.SharedKernel.Exceptions;
using Notification.Domain.Enums;

namespace Notification.Domain.Entities;

/// <summary>
/// A per-(event code, channel) content template with simple
/// <c>{{PlaceholderName}}</c> token substitution — deliberately not a full
/// templating engine, since the placeholder set per event is small and fixed.
/// </summary>
public sealed partial class NotificationTemplate : AuditableEntity<Guid>
{
    /// <summary>The integration event type this template renders for, e.g. "AuditPlanned".</summary>
    public string Code { get; private set; } = default!;

    public NotificationChannel Channel { get; private set; }

    /// <summary>Unused for InApp (no separate subject line concept); required for Email.</summary>
    public string? SubjectTemplate { get; private set; }

    public string BodyTemplate { get; private set; } = default!;

    public bool IsActive { get; private set; } = true;

    private NotificationTemplate()
    {
    }

    public static NotificationTemplate Create(string code, NotificationChannel channel, string? subjectTemplate, string bodyTemplate, string createdBy)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(code))
        {
            errors["code"] = ["Code is required."];
        }

        if (channel == NotificationChannel.Email && string.IsNullOrWhiteSpace(subjectTemplate))
        {
            errors["subjectTemplate"] = ["Subject template is required for Email templates."];
        }

        if (string.IsNullOrWhiteSpace(bodyTemplate))
        {
            errors["bodyTemplate"] = ["Body template is required."];
        }

        if (errors.Count > 0)
        {
            throw new BusinessValidationException(errors);
        }

        return new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            Code = code,
            Channel = channel,
            SubjectTemplate = subjectTemplate,
            BodyTemplate = bodyTemplate,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedDate = DateTime.UtcNow,
        };
    }

    public (string Subject, string Body) Render(IReadOnlyDictionary<string, string> placeholders) =>
        (Substitute(SubjectTemplate ?? string.Empty, placeholders), Substitute(BodyTemplate, placeholders));

    private static string Substitute(string template, IReadOnlyDictionary<string, string> placeholders) =>
        PlaceholderPattern().Replace(template, match =>
            placeholders.TryGetValue(match.Groups[1].Value, out var value) ? value : match.Value);

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex PlaceholderPattern();
}
