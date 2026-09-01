using Finding.Domain.Entities;

namespace Finding.Application.Common;

public interface IActionPlanReferenceRepository
{
    Task<bool> ExistsAsync(Guid findingId, CancellationToken cancellationToken = default);

    void Add(ActionPlanReference reference);
}
