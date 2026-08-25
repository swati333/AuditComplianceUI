using System.Security.Claims;
using Ehs.Observability.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Application.Services;
using Notification.Contracts.Dtos;
using Notification.Contracts.Requests;

namespace Notification.Api.Controllers;

/// <summary>
/// A user manages their own preferences only — <paramref name="userId"/> is
/// caller-supplied routing, not an identity claim, so it must never be
/// trusted on its own (CLAUDE.md §10). Every action checks it against the
/// caller's validated <c>oid</c> claim; an administrator holding
/// <see cref="EhsAuthorizationPolicies.CanManageConfiguration"/> may act on
/// any user's preferences.
/// </summary>
[ApiController]
[Route("api/v1/notification-preferences")]
[Produces("application/json")]
[Authorize]
public sealed class NotificationPreferencesController : ControllerBase
{
    private readonly NotificationPreferenceService _preferenceService;

    public NotificationPreferencesController(NotificationPreferenceService preferenceService)
    {
        _preferenceService = preferenceService;
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(IReadOnlyCollection<NotificationPreferenceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyCollection<NotificationPreferenceDto>>> GetForUser(string userId, CancellationToken cancellationToken)
    {
        if (!CallerCanAccess(userId))
        {
            return Forbid();
        }

        var result = await _preferenceService.GetForUserAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{userId}/{channel}")]
    [ProducesResponseType(typeof(NotificationPreferenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<NotificationPreferenceDto>> Upsert(string userId, string channel, [FromBody] UpsertPreferenceRequest request, CancellationToken cancellationToken)
    {
        if (!CallerCanAccess(userId))
        {
            return Forbid();
        }

        var result = await _preferenceService.UpsertAsync(userId, channel, request.IsEnabled, cancellationToken);
        return Ok(result);
    }

    private bool CallerCanAccess(string userId)
    {
        var callerId = User.FindFirst("oid")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.Equals(callerId, userId, StringComparison.OrdinalIgnoreCase)
            || User.IsInRole(EntraAppRoles.ConfigurationManage);
    }
}
