using Microsoft.EntityFrameworkCore;
using Notification.Application.Common;
using Notification.Domain.Entities;
using Notification.Domain.Enums;

namespace Notification.Infrastructure.Persistence.Repositories;

public sealed class NotificationPreferenceRepository : INotificationPreferenceRepository
{
    private readonly NotificationDbContext _dbContext;

    public NotificationPreferenceRepository(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(NotificationPreference preference) => _dbContext.NotificationPreferences.Add(preference);

    // Tracked (not AsNoTracking): callers use this both to check a preference
    // (read-only) and to mutate it via SetEnabled (NotificationPreferenceService).
    public Task<NotificationPreference?> GetAsync(string userId, NotificationChannel channel, CancellationToken cancellationToken = default) =>
        _dbContext.NotificationPreferences.FirstOrDefaultAsync(p => p.UserId == userId && p.Channel == channel, cancellationToken);

    public async Task<IReadOnlyList<NotificationPreference>> ListForUserAsync(string userId, CancellationToken cancellationToken = default) =>
        await _dbContext.NotificationPreferences.AsNoTracking().Where(p => p.UserId == userId).ToListAsync(cancellationToken);
}
