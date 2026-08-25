using Ehs.SharedKernel.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Application.Services;
using Notification.Contracts.Dtos;
using Notification.Contracts.Requests;

namespace Notification.Api.Controllers;

/// <summary>
/// Routes and verbs follow CLAUDE.md §6 — see Audit.Api's AuditsController
/// for the full convention rationale. Every action requires only an
/// authenticated caller — none of the 7 defined policies target
/// "manage your own notifications", and every operation here is inherently
/// self-scoped (a user's own inbox), so no dedicated policy is warranted.
/// </summary>
[ApiController]
[Route("api/v1/notifications")]
[Produces("application/json")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly NotificationQueryService _notificationQueryService;

    public NotificationsController(NotificationQueryService notificationQueryService)
    {
        _notificationQueryService = notificationQueryService;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _notificationQueryService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<NotificationDto>>> Search([FromQuery] NotificationListQuery query, CancellationToken cancellationToken)
    {
        var result = await _notificationQueryService.SearchAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<NotificationDto>> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        var result = await _notificationQueryService.MarkReadAsync(id, cancellationToken);
        return Ok(result);
    }
}
