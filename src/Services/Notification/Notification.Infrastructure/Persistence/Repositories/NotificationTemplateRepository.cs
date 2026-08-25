using Microsoft.EntityFrameworkCore;
using Notification.Application.Common;
using Notification.Domain.Entities;

namespace Notification.Infrastructure.Persistence.Repositories;

public sealed class NotificationTemplateRepository : INotificationTemplateRepository
{
    private readonly NotificationDbContext _dbContext;

    public NotificationTemplateRepository(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(NotificationTemplate template) => _dbContext.NotificationTemplates.Add(template);

    public async Task<IReadOnlyList<NotificationTemplate>> GetActiveByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        await _dbContext.NotificationTemplates
            .AsNoTracking()
            .Where(t => t.Code == code && t.IsActive)
            .ToListAsync(cancellationToken);

    public Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.NotificationTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<NotificationTemplate>> ListAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.NotificationTemplates.AsNoTracking().OrderBy(t => t.Code).ThenBy(t => t.Channel).ToListAsync(cancellationToken);
}
