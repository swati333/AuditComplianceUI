using Audit.Application.Services;
using Audit.Contracts.Dtos;
using Audit.Contracts.Requests;
using Ehs.Observability.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Audit.Api.Controllers;

/// <summary>
/// Minimal checklist management: enough to make "assign a configurable
/// checklist" (CLAUDE.md functional requirements) work end-to-end. Full
/// checklist update/versioning is out of scope for this phase. Every action
/// requires an authenticated caller; creating a checklist template is an
/// administrative operation gated by
/// <see cref="EhsAuthorizationPolicies.CanManageAudits"/>.
/// </summary>
[ApiController]
[Route("api/v1/checklists")]
[Produces("application/json")]
[Authorize]
public sealed class ChecklistsController : ControllerBase
{
    private readonly ChecklistService _checklistService;

    public ChecklistsController(ChecklistService checklistService)
    {
        _checklistService = checklistService;
    }

    [HttpPost]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageAudits)]
    [ProducesResponseType(typeof(ChecklistDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ChecklistDto>> Create([FromBody] CreateChecklistRequest request, CancellationToken cancellationToken)
    {
        var result = await _checklistService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ChecklistDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChecklistDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _checklistService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<ChecklistDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ChecklistDto>>> List(CancellationToken cancellationToken)
    {
        var result = await _checklistService.ListActiveAsync(cancellationToken);
        return Ok(result);
    }
}
