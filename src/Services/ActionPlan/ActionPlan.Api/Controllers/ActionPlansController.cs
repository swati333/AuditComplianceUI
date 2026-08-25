using ActionPlan.Application.Services;
using ActionPlan.Contracts.Dtos;
using ActionPlan.Contracts.Requests;
using Ehs.Observability.Security;
using Ehs.SharedKernel.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ActionPlan.Api.Controllers;

/// <summary>
/// Routes and verbs follow CLAUDE.md §6 — see Finding.Api's FindingsController
/// for the full convention rationale. Policy split per CLAUDE.md §10's three
/// action-plan-specific roles: record-shape mutations (create/update/delete/
/// close/cancel) require CanManageFindings (the same compliance-manager
/// oversight Finding Service already gates its own mutations behind — an
/// action plan is a corrective action *assigned by* that function); Start/
/// Submit require CanManageOwnActions *and* the domain itself checks the
/// caller is this action's actual owner; Approve/Reject require
/// CanApproveActions, with self-approval prevented in the domain regardless
/// of policy. Comments/evidence stay authenticated-only since an owner also
/// needs to participate without a management-level role.
/// </summary>
[ApiController]
[Route("api/v1/actions")]
[Produces("application/json")]
[Authorize]
public sealed class ActionPlansController : ControllerBase
{
    private readonly ActionPlanService _actionPlanService;

    public ActionPlansController(ActionPlanService actionPlanService)
    {
        _actionPlanService = actionPlanService;
    }

    [HttpPost]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageFindings)]
    [ProducesResponseType(typeof(ActionPlanDetailDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ActionPlanDetailDto>> Create([FromBody] CreateActionPlanRequest request, CancellationToken cancellationToken)
    {
        var result = await _actionPlanService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ActionPlanDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActionPlanDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _actionPlanService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ActionPlanSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ActionPlanSummaryDto>>> Search([FromQuery] ActionPlanListQuery query, CancellationToken cancellationToken)
    {
        var result = await _actionPlanService.SearchAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageFindings)]
    [ProducesResponseType(typeof(ActionPlanDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ActionPlanDetailDto>> Update(Guid id, [FromBody] UpdateActionPlanRequest request, CancellationToken cancellationToken)
    {
        var result = await _actionPlanService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageFindings)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _actionPlanService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/comments")]
    [ProducesResponseType(typeof(ActionPlanCommentDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ActionPlanCommentDto>> AddComment(Guid id, [FromBody] AddActionPlanCommentRequest request, CancellationToken cancellationToken)
    {
        var result = await _actionPlanService.AddCommentAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/evidence")]
    [ProducesResponseType(typeof(ActionPlanEvidenceDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ActionPlanEvidenceDto>> AddEvidence(Guid id, [FromBody] AddActionPlanEvidenceRequest request, CancellationToken cancellationToken)
    {
        var result = await _actionPlanService.AddEvidenceAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/status-history")]
    [ProducesResponseType(typeof(IReadOnlyCollection<ActionPlanStatusHistoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ActionPlanStatusHistoryDto>>> GetStatusHistory(Guid id, CancellationToken cancellationToken)
    {
        var result = await _actionPlanService.GetStatusHistoryAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/start")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageOwnActions)]
    [ProducesResponseType(typeof(ActionPlanDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ActionPlanDetailDto>> Start(Guid id, CancellationToken cancellationToken)
    {
        var result = await _actionPlanService.StartAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/submit")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageOwnActions)]
    [ProducesResponseType(typeof(ActionPlanDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ActionPlanDetailDto>> Submit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _actionPlanService.SubmitAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanApproveActions)]
    [ProducesResponseType(typeof(ActionPlanDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ActionPlanDetailDto>> Approve(Guid id, CancellationToken cancellationToken)
    {
        var result = await _actionPlanService.ApproveAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanApproveActions)]
    [ProducesResponseType(typeof(ActionPlanDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ActionPlanDetailDto>> Reject(Guid id, [FromBody] RejectActionPlanRequest request, CancellationToken cancellationToken)
    {
        var result = await _actionPlanService.RejectAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/close")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageFindings)]
    [ProducesResponseType(typeof(ActionPlanDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ActionPlanDetailDto>> Close(Guid id, CancellationToken cancellationToken)
    {
        var result = await _actionPlanService.CloseAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageFindings)]
    [ProducesResponseType(typeof(ActionPlanDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ActionPlanDetailDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _actionPlanService.CancelAsync(id, cancellationToken);
        return Ok(result);
    }
}
