using ActionPlan.Domain.Entities;
using ActionPlan.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using ActionPlanEntity = ActionPlan.Domain.Entities.ActionPlan;

namespace ActionPlan.Infrastructure.Persistence;

/// <summary>
/// Runtime seeding (not <c>HasData</c>) so the aggregate factory methods run
/// normally and enforce their own invariants. The FindingIds used here are
/// synthetic (not really produced by a running Finding Service) — in
/// production these rows would only ever be created by consuming
/// FindingCreated events, never invented locally like this; this is seed
/// data only, standing in for "events we would have already consumed" — see
/// Finding Service's identical SeedData for the same rationale.
/// </summary>
public static class SeedData
{
    private const string SeedUser = "system@seed";
    private const string OwnerId = "33333333-3333-3333-3333-333333333333";
    private const string ApproverId = "44444444-4444-4444-4444-444444444444";

    public static async Task SeedAsync(ActionPlanDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (await dbContext.FindingReferences.AnyAsync(cancellationToken))
        {
            return;
        }

        var criticalFindingId = Guid.NewGuid();
        var highFindingId = Guid.NewGuid();
        dbContext.FindingReferences.Add(FindingReference.Create(criticalFindingId, "Blocked emergency exit in warehouse bay 3"));
        dbContext.FindingReferences.Add(FindingReference.Create(highFindingId, "Expired fire extinguisher inspection tags"));

        var assignedAction = ActionPlanEntity.Create(
            criticalFindingId,
            ActionType.Corrective,
            "Clear and re-mark emergency exit egress path",
            "Remove stored pallets from the east emergency exit and install floor marking to prevent recurrence.",
            OwnerId,
            "Jordan Rivera",
            ApproverId,
            "Morgan Lee",
            DateTime.UtcNow.AddDays(7),
            ActionPriority.Critical,
            SeedUser);
        dbContext.ActionPlans.Add(assignedAction);

        var inProgressAction = ActionPlanEntity.Create(
            highFindingId,
            ActionType.Corrective,
            "Renew fire extinguisher inspection contract",
            "Re-engage a certified vendor for monthly extinguisher inspections.",
            OwnerId,
            "Jordan Rivera",
            ApproverId,
            "Morgan Lee",
            DateTime.UtcNow.AddDays(-2),
            ActionPriority.High,
            SeedUser);
        inProgressAction.Start(OwnerId, SeedUser);
        dbContext.ActionPlans.Add(inProgressAction);

        var overdueAction = ActionPlanEntity.Create(
            highFindingId,
            ActionType.Preventive,
            "Add extinguisher inspection to preventive maintenance schedule",
            null,
            OwnerId,
            "Jordan Rivera",
            ApproverId,
            "Morgan Lee",
            DateTime.UtcNow.AddDays(-10),
            ActionPriority.Medium,
            SeedUser);
        overdueAction.MarkOverdue(DateTime.UtcNow);
        dbContext.ActionPlans.Add(overdueAction);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
