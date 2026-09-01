using Audit.Domain.Entities;

namespace Audit.Application.Common;

public interface IFindingReferenceRepository
{
    Task<FindingReference?> GetByFindingIdAsync(Guid findingId, CancellationToken cancellationToken = default);

    void Add(FindingReference reference);
}
