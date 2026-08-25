using Finding.Domain.Entities;
using Finding.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using FindingEntity = Finding.Domain.Entities.Finding;

namespace Finding.Infrastructure.Persistence;

/// <summary>
/// Runtime seeding (not <c>HasData</c>) so the aggregate factory methods run
/// normally and enforce their own invariants. The AuditIds used here are
/// synthetic (not really produced by a running Audit Service) — in
/// production these rows would only ever be created by consuming
/// AuditCreated events, never invented locally like this; this is seed data
/// only, standing in for "events we would have already consumed."
/// </summary>
public static class SeedData
{
    private const string SeedUser = "system@seed";

    public static async Task SeedAsync(FindingDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.AuditReferences.AnyAsync(cancellationToken))
        {
            return;
        }

        var auditOneId = Guid.NewGuid();
        var auditTwoId = Guid.NewGuid();
        dbContext.AuditReferences.Add(AuditReference.Create(auditOneId, "Q1 Fire Safety Inspection - Plant A"));
        dbContext.AuditReferences.Add(AuditReference.Create(auditTwoId, "Annual Environmental Compliance Audit"));

        var criticalFinding = FindingEntity.Create(
            auditOneId,
            "Blocked emergency exit in warehouse bay 3",
            "Pallets stacked in front of the east emergency exit, blocking egress.",
            FindingSeverity.Critical,
            SeedUser);
        dbContext.Findings.Add(criticalFinding);

        var highFinding = FindingEntity.Create(
            auditOneId,
            "Expired fire extinguisher inspection tags",
            "Three extinguishers on the production floor have inspection tags over a year old.",
            FindingSeverity.High,
            SeedUser);
        highFinding.RecordRootCauseAnalysis("Inspection vendor contract lapsed and was not renewed on schedule.", SeedUser);
        dbContext.Findings.Add(highFinding);

        var lowFinding = FindingEntity.Create(
            auditTwoId,
            "Minor housekeeping issue in break room",
            "Recycling bin not clearly labeled.",
            FindingSeverity.Low,
            SeedUser);
        dbContext.Findings.Add(lowFinding);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
