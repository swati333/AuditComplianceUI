using Ehs.SharedKernel.Exceptions;
using Ehs.SharedKernel.Pagination;
using Notification.Application.Common;
using Notification.Application.Mapping;
using Notification.Contracts.Dtos;
using Notification.Contracts.Requests;

namespace Notification.Application.Services;

public sealed class NotificationQueryService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public NotificationQueryService(INotificationRepository notificationRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<NotificationDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetDetailAsync(id, cancellationToken) ?? throw NotFoundException.For("Notification", id);
        return notification.ToDto();
    }

    public async Task<PagedResult<NotificationDto>> SearchAsync(NotificationListQuery query, CancellationToken cancellationToken = default)
    {
        var paging = new PagedRequest
        {
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            SortBy = query.SortBy,
            SortDirection = string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase)
                ? SortDirection.Ascending
                : SortDirection.Descending,
        };
        query.PageNumber = paging.PageNumber;
        query.PageSize = paging.PageSize;

        var (items, totalCount) = await _notificationRepository.SearchAsync(query, cancellationToken);
        var dtos = items.Select(n => n.ToDto()).ToList();
        return new PagedResult<NotificationDto>(dtos, paging.PageNumber, paging.PageSize, totalCount);
    }

    public async Task<NotificationDto> MarkReadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetTrackedByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("Notification", id);
        notification.MarkRead(_currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return notification.ToDto();
    }
}
