using Ehs.SharedKernel.Exceptions;
using Notification.Application.Common;
using Notification.Application.Mapping;
using Notification.Contracts.Dtos;

namespace Notification.Application.Services;

/// <summary>Read-only: templates are configured via seed data in this phase, not a management API — see SeedData for the full set.</summary>
public sealed class NotificationTemplateQueryService
{
    private readonly INotificationTemplateRepository _templateRepository;

    public NotificationTemplateQueryService(INotificationTemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<NotificationTemplateDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await _templateRepository.GetByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("NotificationTemplate", id);
        return template.ToDto();
    }

    public async Task<IReadOnlyCollection<NotificationTemplateDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var templates = await _templateRepository.ListAsync(cancellationToken);
        return templates.Select(t => t.ToDto()).ToList();
    }
}
