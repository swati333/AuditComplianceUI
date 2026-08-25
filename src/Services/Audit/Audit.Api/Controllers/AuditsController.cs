using Audit.Application.Services;
using Audit.Contracts.Dtos;
using Audit.Contracts.Requests;
using Ehs.Observability.Security;
using Ehs.SharedKernel.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Audit.Api.Controllers;

/// <summary>
/// Routes and verbs follow CLAUDE.md §6: versioned under /api/v1, GET for
/// list/detail, POST for create and sub-resource actions/transitions, PUT
/// for full update, DELETE for soft delete. Every action requires at least
/// an authenticated caller (CLAUDE.md §10: "no unauthenticated business
/// routes"); structural/administrative operations additionally require
/// <see cref="EhsAuthorizationPolicies.CanManageAudits"/>, and fieldwork
/// actions an Auditor performs during execution require
/// <see cref="EhsAuthorizationPolicies.CanPerformAudits"/>. Reads require
/// only authentication, since Auditees also need visibility into audits.
/// </summary>
[ApiController]
[Route("api/v1/audits")]
[Produces("application/json")]
[Authorize]
public sealed class AuditsController : ControllerBase
{
    private readonly AuditService _auditService;

    public AuditsController(AuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpPost]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageAudits)]
    [ProducesResponseType(typeof(AuditDetailDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AuditDetailDto>> Create([FromBody] CreateAuditRequest request, CancellationToken cancellationToken)
    {
        var result = await _auditService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AuditDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuditDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _auditService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AuditSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AuditSummaryDto>>> Search([FromQuery] AuditListQuery query, CancellationToken cancellationToken)
    {
        var result = await _auditService.SearchAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageAudits)]
    [ProducesResponseType(typeof(AuditDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuditDetailDto>> Update(Guid id, [FromBody] UpdateAuditRequest request, CancellationToken cancellationToken)
    {
        var result = await _auditService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageAudits)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _auditService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/team-members")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageAudits)]
    [ProducesResponseType(typeof(AuditTeamMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuditTeamMemberDto>> AssignTeamMember(Guid id, [FromBody] AssignTeamMemberRequest request, CancellationToken cancellationToken)
    {
        var result = await _auditService.AssignTeamMemberAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}/team-members/{teamMemberId:guid}")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageAudits)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveTeamMember(Guid id, Guid teamMemberId, CancellationToken cancellationToken)
    {
        await _auditService.RemoveTeamMemberAsync(id, teamMemberId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/checklist")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageAudits)]
    [ProducesResponseType(typeof(AuditDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuditDetailDto>> AssignChecklist(Guid id, [FromBody] AssignChecklistRequest request, CancellationToken cancellationToken)
    {
        var result = await _auditService.AssignChecklistAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/checklist-responses")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanPerformAudits)]
    [ProducesResponseType(typeof(ChecklistResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ChecklistResponseDto>> RecordChecklistResponse(Guid id, [FromBody] RecordChecklistResponseRequest request, CancellationToken cancellationToken)
    {
        var result = await _auditService.RecordChecklistResponseAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/status-history")]
    [ProducesResponseType(typeof(IReadOnlyCollection<AuditStatusHistoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<AuditStatusHistoryDto>>> GetStatusHistory(Guid id, CancellationToken cancellationToken)
    {
        var result = await _auditService.GetStatusHistoryAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/plan")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageAudits)]
    [ProducesResponseType(typeof(AuditDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuditDetailDto>> Plan(Guid id, [FromBody] PlanAuditRequest request, CancellationToken cancellationToken)
    {
        var result = await _auditService.PlanAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/start")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanPerformAudits)]
    [ProducesResponseType(typeof(AuditDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuditDetailDto>> Start(Guid id, CancellationToken cancellationToken)
    {
        var result = await _auditService.StartAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/complete")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanPerformAudits)]
    [ProducesResponseType(typeof(AuditDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuditDetailDto>> Complete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _auditService.CompleteAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/close")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageAudits)]
    [ProducesResponseType(typeof(AuditDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuditDetailDto>> Close(Guid id, CancellationToken cancellationToken)
    {
        var result = await _auditService.CloseAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageAudits)]
    [ProducesResponseType(typeof(AuditDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuditDetailDto>> Cancel(Guid id, [FromBody] CancelAuditRequest request, CancellationToken cancellationToken)
    {
        var result = await _auditService.CancelAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
