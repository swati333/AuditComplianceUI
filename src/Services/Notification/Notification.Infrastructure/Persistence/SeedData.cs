using Microsoft.EntityFrameworkCore;
using Notification.Domain.Entities;
using Notification.Domain.Enums;

namespace Notification.Infrastructure.Persistence;

/// <summary>Seeds one Email + one InApp template per consumed event code — the full set NotificationFactory looks up by <c>eventCode</c>.</summary>
public static class SeedData
{
    private const string SeedUser = "system@seed";

    public static async Task SeedAsync(NotificationDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.NotificationTemplates.AnyAsync(cancellationToken))
        {
            return;
        }

        AddTemplatePair(
            dbContext,
            "AuditPlanned",
            "Audit planned: {{AuditId}}",
            "An audit has been planned for {{PlannedStartDate}} to {{PlannedEndDate}}. Audit ID: {{AuditId}}.");

        AddTemplatePair(
            dbContext,
            "CriticalFindingCreated",
            "CRITICAL finding raised: {{Title}}",
            "A Critical-severity finding was raised on audit {{AuditId}}: \"{{Title}}\" (Finding ID: {{FindingId}}). Immediate attention required.");

        AddTemplatePair(
            dbContext,
            "ActionPlanAssigned",
            "Corrective action assigned to you: {{Title}}",
            "You have been assigned a corrective action \"{{Title}}\" for finding {{FindingId}}, due {{DueDate}}. Action Plan ID: {{ActionPlanId}}.");

        AddTemplatePair(
            dbContext,
            "ActionPlanSubmitted",
            "Your corrective action was submitted for approval",
            "Your corrective action (Action Plan ID: {{ActionPlanId}}) for finding {{FindingId}} has been submitted for approval.");

        AddTemplatePair(
            dbContext,
            "ActionPlanRejected",
            "Corrective action rejected — action needed",
            "Your corrective action (Action Plan ID: {{ActionPlanId}}) for finding {{FindingId}} was rejected by {{RejectedByUserId}}: {{Reason}}. Please revise and resubmit.");

        AddTemplatePair(
            dbContext,
            "ActionPlanOverdue",
            "Corrective action overdue",
            "Your corrective action (Action Plan ID: {{ActionPlanId}}) for finding {{FindingId}} was due {{DueDate}} and is now overdue.");

        AddTemplatePair(
            dbContext,
            "ReportGenerated",
            "Your report is ready: {{ReportType}}",
            "Your {{ReportType}} report (Report ID: {{ReportId}}) has finished generating and is ready to download.");

        AddTemplatePair(
            dbContext,
            "ReportGenerationFailed",
            "Report generation failed: {{ReportType}}",
            "Your {{ReportType}} report (Report ID: {{ReportId}}) failed to generate: {{ErrorMessage}}. Please try again or contact support.");

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void AddTemplatePair(NotificationDbContext dbContext, string code, string subject, string body)
    {
        dbContext.NotificationTemplates.Add(NotificationTemplate.Create(code, NotificationChannel.Email, subject, body, SeedUser));
        dbContext.NotificationTemplates.Add(NotificationTemplate.Create(code, NotificationChannel.InApp, subjectTemplate: null, body, SeedUser));
    }
}
