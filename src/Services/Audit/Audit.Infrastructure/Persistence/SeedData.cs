using Audit.Domain.Entities;
using Audit.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using AuditEntity = Audit.Domain.Entities.Audit;

namespace Audit.Infrastructure.Persistence;

/// <summary>
/// Runtime seeding (not <c>HasData</c>) so aggregate factory methods run
/// normally and enforce their own invariants, rather than seeding raw column
/// values that could bypass them. Idempotent: no-ops if any checklist
/// already exists.
/// </summary>
public static class SeedData
{
    private const string SeedUser = "system@seed";

    public static async Task SeedAsync(AuditDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Checklists.IgnoreQueryFilters().AnyAsync(cancellationToken))
        {
            return;
        }

        var checklist = Checklist.Create(
            "ISO 45001 Workplace Safety Checklist",
            "Standard workplace safety inspection checklist.",
            SeedUser);
        checklist.AddQuestion("Are all emergency exits clearly marked and unobstructed?", isMandatory: true, displayOrder: 1, SeedUser);
        checklist.AddQuestion("Is firefighting equipment inspected and in date?", isMandatory: true, displayOrder: 2, SeedUser);
        checklist.AddQuestion("Are PPE requirements posted and followed?", isMandatory: true, displayOrder: 3, SeedUser);
        checklist.AddQuestion("Is the first aid kit fully stocked?", isMandatory: false, displayOrder: 4, SeedUser);
        checklist.AddQuestion("Are walkways and floors free of spills and obstructions?", isMandatory: true, displayOrder: 5, SeedUser);
        dbContext.Checklists.Add(checklist);

        var draftAudit = AuditEntity.Create(
            "Q1 Fire Safety Inspection - Plant A",
            "Quarterly fire safety compliance inspection for Plant A production floor.",
            "Production floor, warehouse, and loading dock areas",
            "Plant A - Springfield, IL",
            SeedUser);
        dbContext.Audits.Add(draftAudit);

        var plannedAudit = AuditEntity.Create(
            "Annual Environmental Compliance Audit",
            "Full-scope annual EHS environmental compliance audit.",
            "Wastewater treatment, air emissions, hazardous waste storage",
            "Plant B - Dayton, OH",
            SeedUser);
        plannedAudit.AssignTeamMember("seed-auditor-1", "Alex Auditor", AuditTeamRole.Auditor, SeedUser);
        plannedAudit.AssignTeamMember("seed-auditee-1", "Sam Auditee", AuditTeamRole.Auditee, SeedUser);
        plannedAudit.AssignChecklist(checklist.Id, SeedUser);
        plannedAudit.Plan(DateTime.UtcNow.Date.AddDays(7), DateTime.UtcNow.Date.AddDays(9), SeedUser);
        dbContext.Audits.Add(plannedAudit);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
