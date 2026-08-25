using ActionPlan.Domain.Entities;

namespace ActionPlan.Application.Common;

public interface IFindingReferenceRepository
{
    Task<FindingReference?> GetByIdAsync(Guid findingId, CancellationToken cancellationToken = default);

    void Add(FindingReference reference);
}
