using Ehs.Observability.Security;
using Ehs.SharedKernel.Pagination;
using Finding.Application.Services;
using Finding.Contracts.Dtos;
using Finding.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finding.Api.Controllers;

/// <summary>
/// Routes and verbs follow CLAUDE.md §6 — see Audit.Api's AuditsController
/// for the full convention rationale. Every action requires an authenticated
/// caller; mutations to the finding record and its lifecycle transitions
/// additionally require <see cref="EhsAuthorizationPolicies.CanManageFindings"/>.
/// Comments/documents stay authenticated-only since an Auditee also needs to
/// participate in the finding's resolution.
/// </summary>
[ApiController]
[Route("api/v1/findings")]
[Produces("application/json")]
[Authorize]
public sealed class FindingsController : ControllerBase
{
    private readonly FindingService _findingService;

    public FindingsController(FindingService findingService)
    {
        _findingService = findingService;
    }

    [HttpPost]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageFindings)]
    [ProducesResponseType(typeof(FindingDetailDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<FindingDetailDto>> Create([FromBody] CreateFindingRequest request, CancellationToken cancellationToken)
    {
        var result = await _findingService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FindingDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FindingDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _findingService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<FindingSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<FindingSummaryDto>>> Search([FromQuery] FindingListQuery query, CancellationToken cancellationToken)
    {
        var result = await _findingService.SearchAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageFindings)]
    [ProducesResponseType(typeof(FindingDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FindingDetailDto>> Update(Guid id, [FromBody] UpdateFindingRequest request, CancellationToken cancellationToken)
    {
        var result = await _findingService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageFindings)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _findingService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/root-cause-analysis")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageFindings)]
    [ProducesResponseType(typeof(FindingDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<FindingDetailDto>> RecordRootCauseAnalysis(Guid id, [FromBody] RecordRootCauseAnalysisRequest request, CancellationToken cancellationToken)
    {
        var result = await _findingService.RecordRootCauseAnalysisAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/comments")]
    [ProducesResponseType(typeof(FindingCommentDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<FindingCommentDto>> AddComment(Guid id, [FromBody] AddCommentRequest request, CancellationToken cancellationToken)
    {
        var result = await _findingService.AddCommentAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/documents")]
    [ProducesResponseType(typeof(FindingDocumentDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<FindingDocumentDto>> AddDocument(Guid id, [FromBody] AddDocumentRequest request, CancellationToken cancellationToken)
    {
        var result = await _findingService.AddDocumentAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/status-history")]
    [ProducesResponseType(typeof(IReadOnlyCollection<FindingStatusHistoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<FindingStatusHistoryDto>>> GetStatusHistory(Guid id, CancellationToken cancellationToken)
    {
        var result = await _findingService.GetStatusHistoryAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/start-review")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageFindings)]
    [ProducesResponseType(typeof(FindingDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FindingDetailDto>> StartReview(Guid id, CancellationToken cancellationToken)
    {
        var result = await _findingService.StartReviewAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/require-action")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageFindings)]
    [ProducesResponseType(typeof(FindingDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FindingDetailDto>> RequireAction(Guid id, CancellationToken cancellationToken)
    {
        var result = await _findingService.RequireActionAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/resolve")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageFindings)]
    [ProducesResponseType(typeof(FindingDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FindingDetailDto>> Resolve(Guid id, CancellationToken cancellationToken)
    {
        var result = await _findingService.ResolveAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/verify")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageFindings)]
    [ProducesResponseType(typeof(FindingDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FindingDetailDto>> Verify(Guid id, CancellationToken cancellationToken)
    {
        var result = await _findingService.VerifyAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/close")]
    [Authorize(Policy = EhsAuthorizationPolicies.CanManageFindings)]
    [ProducesResponseType(typeof(FindingDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FindingDetailDto>> Close(Guid id, CancellationToken cancellationToken)
    {
        var result = await _findingService.CloseAsync(id, cancellationToken);
        return Ok(result);
    }
}
