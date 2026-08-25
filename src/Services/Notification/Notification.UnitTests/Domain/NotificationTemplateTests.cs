using Ehs.SharedKernel.Exceptions;
using FluentAssertions;
using Notification.Domain.Enums;
using TemplateEntity = Notification.Domain.Entities.NotificationTemplate;

namespace Notification.UnitTests.Domain;

public class NotificationTemplateTests
{
    private const string Actor = "system";

    [Fact]
    public void Create_requires_a_subject_template_for_Email()
    {
        var act = () => TemplateEntity.Create("Code", NotificationChannel.Email, subjectTemplate: null, "Body", Actor);

        act.Should().Throw<BusinessValidationException>();
    }

    [Fact]
    public void Create_does_not_require_a_subject_template_for_InApp()
    {
        var template = TemplateEntity.Create("Code", NotificationChannel.InApp, subjectTemplate: null, "Body", Actor);

        template.SubjectTemplate.Should().BeNull();
    }

    [Fact]
    public void Render_substitutes_known_placeholders()
    {
        var template = TemplateEntity.Create("Code", NotificationChannel.Email, "Audit {{AuditId}} planned", "Starts {{PlannedStartDate}}.", Actor);

        var (subject, body) = template.Render(new Dictionary<string, string>
        {
            ["AuditId"] = "abc-123",
            ["PlannedStartDate"] = "2026-09-01",
        });

        subject.Should().Be("Audit abc-123 planned");
        body.Should().Be("Starts 2026-09-01.");
    }

    [Fact]
    public void Render_leaves_unknown_placeholders_untouched()
    {
        var template = TemplateEntity.Create("Code", NotificationChannel.InApp, null, "Value: {{Unknown}}", Actor);

        var (_, body) = template.Render(new Dictionary<string, string>());

        body.Should().Be("Value: {{Unknown}}");
    }
}
