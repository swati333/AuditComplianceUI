using ActionPlan.Application.Common;
using ActionPlan.Application.Mapping;
using ActionPlan.Contracts.Dtos;
using ActionPlan.Contracts.Requests;
using ActionPlan.Domain.Enums;
using Ehs.SharedKernel.Exceptions;
using Ehs.SharedKernel.Pagination;
using ActionPlanEntity = ActionPlan.Domain.Entities.ActionPlan;

namespace ActionPlan.Application.Services;

/// <summary>
/// Application-layer use cases for the ActionPlan aggregate. A plain service
/// class rather than MediatR/CQRS handlers (CLAUDE.md §4), mirroring Finding
/// Service's FindingService.
/// </summary>
public sealed class ActionPlanService
{
    private readonly IActionPlanRepository _actionPlanRepository;
    private readonly IFindingReferenceRepository _findingReferenceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ActionPlanService(
        IActionPlanRepository actionPlanRepository,
        IFindingReferenceRepository findingReferenceRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _actionPlanRepository = actionPlanRepository;
        _findingReferenceRepository = findingReferenceRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<ActionPlanDetailDto> CreateAsync(CreateActionPlanRequest request, CancellationToken cancellationToken = default)
    {
        // Shape (are these known enum names) is already checked by
        // CreateActionPlanRequestValidator; parsing here is safe.
        Enum.TryParse<ActionType>(request.ActionType, ignoreCase: true, out var actionType);
        Enum.TryParse<ActionPriority>(request.Priority, ignoreCase: true, out var priority);

        // Fail closed only on what we positively know (a locally-cached
        // reference marked Closed); fail open if we haven't seen the
        // finding's events yet — see Finding Service's AuditReference for
        // the same eventual-consistency rationale.
        var findingReference = await _findingReferenceRepository.GetByIdAsync(request.FindingId, cancellationToken);
        if (findingReference is { IsClosed: true })
        {
            throw new ConflictException("Cannot create an action plan for a closed finding.", "FINDING_CLOSED");
        }

        var actionPlan = ActionPlanEntity.Create(
            request.FindingId,
            actionType,
            request.Title,
            request.Description,
            request.OwnerId,
            request.OwnerName,
            request.ApproverId,
            request.ApproverName,
            request.DueDate,
            priority,
            _currentUser.UserId);

        _actionPlanRepository.Add(actionPlan);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return actionPlan.ToDetailDto();
    }

    public async Task<ActionPlanDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actionPlan = await _actionPlanRepository.GetDetailAsync(id, cancellationToken) ?? throw NotFoundException.For("ActionPlan", id);
        return actionPlan.ToDetailDto();
    }

    public async Task<PagedResult<ActionPlanSummaryDto>> SearchAsync(ActionPlanListQuery query, CancellationToken cancellationToken = default)
    {
        var paging = new PagedRequest
        {
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            SortBy = query.SortBy,
            SortDirection = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase)
                ? SortDirection.Descending
                : SortDirection.Ascending,
            SearchText = query.SearchText,
        };
        query.PageNumber = paging.PageNumber;
        query.PageSize = paging.PageSize;

        var (items, totalCount) = await _actionPlanRepository.SearchAsync(query, cancellationToken);
        var dtos = items.Select(a => a.ToSummaryDto()).ToList();
        return new PagedResult<ActionPlanSummaryDto>(dtos, paging.PageNumber, paging.PageSize, totalCount);
    }

    public async Task<ActionPlanDetailDto> UpdateAsync(Guid id, UpdateActionPlanRequest request, CancellationToken cancellationToken = default)
    {
        Enum.TryParse<ActionType>(request.ActionType, ignoreCase: true, out var actionType);
        Enum.TryParse<ActionPriority>(request.Priority, ignoreCase: true, out var priority);

        var actionPlan = await _actionPlanRepository.GetTrackedByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("ActionPlan", id);
        _actionPlanRepository.SetRowVersion(actionPlan, request.RowVersion);
        actionPlan.UpdateDetails(
            actionType,
            request.Title,
            request.Description,
            request.OwnerId,
            request.OwnerName,
            request.ApproverId,
            request.ApproverName,
            request.DueDate,
            priority,
            _currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return actionPlan.ToDetailDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actionPlan = await _actionPlanRepository.GetTrackedByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("ActionPlan", id);
        actionPlan.SoftDelete(_currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<ActionPlanCommentDto> AddCommentAsync(Guid id, AddActionPlanCommentRequest request, CancellationToken cancellationToken = default)
    {
        var actionPlan = await _actionPlanRepository.GetTrackedWithDetailsByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("ActionPlan", id);
        var comment = actionPlan.AddComment(request.AuthorId, request.AuthorName, request.Text, _currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return comment.ToDto();
    }

    public async Task<ActionPlanEvidenceDto> AddEvidenceAsync(Guid id, AddActionPlanEvidenceRequest request, CancellationToken cancellationToken = default)
    {
        var actionPlan = await _actionPlanRepository.GetTrackedWithDetailsByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("ActionPlan", id);
        var evidence = actionPlan.AddEvidence(request.FileName, request.BlobReference, request.ContentType, request.SizeBytes, _currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return evidence.ToDto();
    }

    public async Task<IReadOnlyCollection<ActionPlanStatusHistoryDto>> GetStatusHistoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actionPlan = await _actionPlanRepository.GetDetailAsync(id, cancellationToken) ?? throw NotFoundException.For("ActionPlan", id);
        return actionPlan.StatusHistory.OrderBy(h => h.ChangedAtUtc).Select(h => h.ToDto()).ToList();
    }

    public async Task<ActionPlanDetailDto> StartAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actionPlan = await _actionPlanRepository.GetTrackedByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("ActionPlan", id);
        actionPlan.Start(_currentUser.UserId, _currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return actionPlan.ToDetailDto();
    }

    public async Task<ActionPlanDetailDto> SubmitAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actionPlan = await _actionPlanRepository.GetTrackedByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("ActionPlan", id);
        actionPlan.Submit(_currentUser.UserId, _currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return actionPlan.ToDetailDto();
    }

    public async Task<ActionPlanDetailDto> ApproveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actionPlan = await _actionPlanRepository.GetTrackedByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("ActionPlan", id);
        actionPlan.Approve(_currentUser.UserId, _currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return actionPlan.ToDetailDto();
    }

    public async Task<ActionPlanDetailDto> RejectAsync(Guid id, RejectActionPlanRequest request, CancellationToken cancellationToken = default)
    {
        var actionPlan = await _actionPlanRepository.GetTrackedByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("ActionPlan", id);
        actionPlan.Reject(_currentUser.UserId, request.Reason, _currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return actionPlan.ToDetailDto();
    }

    public async Task<ActionPlanDetailDto> CloseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actionPlan = await _actionPlanRepository.GetTrackedByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("ActionPlan", id);
        actionPlan.Close(_currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return actionPlan.ToDetailDto();
    }

    public async Task<ActionPlanDetailDto> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var actionPlan = await _actionPlanRepository.GetTrackedByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("ActionPlan", id);
        actionPlan.Cancel(_currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return actionPlan.ToDetailDto();
    }
}
