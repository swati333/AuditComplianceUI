using Audit.Domain.Entities;

namespace Audit.Application.Common;

public interface IChecklistRepository
{
    Task<Checklist?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Checklist>> ListActiveAsync(CancellationToken cancellationToken = default);

    void Add(Checklist checklist);
}
