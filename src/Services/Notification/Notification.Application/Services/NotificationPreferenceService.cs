using Ehs.SharedKernel.Exceptions;
using Notification.Application.Common;
using Notification.Application.Mapping;
using Notification.Contracts.Dtos;
using Notification.Domain.Enums;
using NotificationPreferenceEntity = Notification.Domain.Entities.NotificationPreference;

namespace Notification.Application.Services;

public sealed class NotificationPreferenceService
{
    private readonly INotificationPreferenceRepository _preferenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public NotificationPreferenceService(INotificationPreferenceRepository preferenceRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _preferenceRepository = preferenceRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyCollection<NotificationPreferenceDto>> GetForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var preferences = await _preferenceRepository.ListForUserAsync(userId, cancellationToken);
        var byChannel = preferences.ToDictionary(p => p.Channel);

        // Every channel is reported explicitly — absence of a stored row
        // means "enabled" (the opt-out default; see NotificationFactory).
        return Enum.GetValues<NotificationChannel>()
            .Select(channel => byChannel.TryGetValue(channel, out var pref)
                ? pref.ToDto()
                : new NotificationPreferenceDto(channel.ToString(), IsEnabled: true))
            .ToList();
    }

    public async Task<NotificationPreferenceDto> UpsertAsync(string userId, string channelName, bool isEnabled, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<NotificationChannel>(channelName, ignoreCase: true, out var channel))
        {
            throw new BusinessValidationException(new Dictionary<string, string[]>
            {
                ["channel"] = [$"Channel must be one of: {string.Join(", ", Enum.GetNames<NotificationChannel>())}."],
            });
        }

        var existing = await _preferenceRepository.GetAsync(userId, channel, cancellationToken);

        if (existing is not null)
        {
            existing.SetEnabled(isEnabled, _currentUser.UserId);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return existing.ToDto();
        }

        var preference = NotificationPreferenceEntity.Create(userId, channel, isEnabled, _currentUser.UserId);
        _preferenceRepository.Add(preference);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return preference.ToDto();
    }
}
