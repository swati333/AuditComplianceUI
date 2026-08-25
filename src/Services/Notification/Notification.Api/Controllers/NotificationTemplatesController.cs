using Ehs.Observability.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Application.Services;
using Notification.Contracts.Dtos;

namespace Notification.Api.Controllers;

/// <summary>
/// Read-only — templates are seed data in this phase (see
/// Notification.Infrastructure's SeedData). Template content is
/// configuration data, so viewing it requires
/// <see cref="EhsAuthorizationPolicies.CanManageConfiguration"/> rather than
/// being open to every authenticated user.
/// </summary>
[ApiController]
[Route("api/v1/notification-templates")]
[Produces("application/json")]
[Authorize(Policy = EhsAuthorizationPolicies.CanManageConfiguration)]
public sealed class NotificationTemplatesController : ControllerBase
{
    private readonly NotificationTemplateQueryService _templateQueryService;

    public NotificationTemplatesController(NotificationTemplateQueryService templateQueryService)
    {
        _templateQueryService = templateQueryService;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(NotificationTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationTemplateDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _templateQueryService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<NotificationTemplateDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<NotificationTemplateDto>>> List(CancellationToken cancellationToken)
    {
        var result = await _templateQueryService.ListAsync(cancellationToken);
        return Ok(result);
    }
}
