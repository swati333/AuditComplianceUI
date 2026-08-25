using Audit.Application.Common;
using Audit.Application.Mapping;
using Audit.Contracts.Dtos;
using Audit.Contracts.Requests;
using Audit.Domain.Enums;
using Ehs.SharedKernel.Exceptions;
using Ehs.SharedKernel.Pagination;
using AuditEntity = Audit.Domain.Entities.Audit;

namespace Audit.Application.Services;

/// <summary>
/// Application-layer use cases for the Audit aggregate. A plain service
/// class rather than MediatR/CQRS handlers — CLAUDE.md §4 says not to add
/// that ceremony unless complexity genuinely justifies it, and it doesn't
/// here. All business rules live on the <see cref="AuditEntity"/> aggregate
/// itself; this class only orchestrates loading, calling into the aggregate,
/// and persisting.
///
/// Which repository read to use per command matters: any aggregate method
/// that inspects an owned collection (team members, checklist responses)
/// needs that collection actually loaded into the tracked instance, or the
/// in-memory invariant check silently sees an empty list instead of what's
/// really in the database.
/// </summary>
public sealed class AuditService
{
    private readonly IAuditRepository _auditRepository;
    private readonly IChecklistRepository _checklistRepository;
    private readonly IAuditComplianceGateway _complianceGateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public AuditService(
        IAuditRepository auditRepository,
        IChecklistRepository checklistRepository,
        IAuditComplianceGateway complianceGateway,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _auditRepository = auditRepository;
        _checklistRepository = checklistRepository;
        _complianceGateway = complianceGateway;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<AuditDetailDto> CreateAsync(CreateAuditRequest request, CancellationToken cancellationToken = default)
    {
        var audit = AuditEntity.Create(request.Title, request.Description, request.Scope, request.Location, _currentUser.UserId);
        _auditRepository.Add(audit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return audit.ToDetailDto();
    }

    public async Task<AuditDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var audit = await _auditRepository.GetDetailAsync(id, cancellationToken) ?? throw NotFoundException.For("Audit", id);
        return audit.ToDetailDto();
    }

    public async Task<PagedResult<AuditSummaryDto>> SearchAsync(AuditListQuery query, CancellationToken cancellationToken = default)
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

        var (items, totalCount) = await _auditRepository.SearchAsync(query, cancellationToken);
        var dtos = items.Select(a => a.ToSummaryDto()).ToList();
        return new PagedResult<AuditSummaryDto>(dtos, paging.PageNumber, paging.PageSize, totalCount);
    }

    public async Task<AuditDetailDto> UpdateAsync(Guid id, UpdateAuditRequest request, CancellationToken cancellationToken = default)
    {
        var audit = await _auditRepository.GetTrackedByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("Audit", id);
        _auditRepository.SetRowVersion(audit, request.RowVersion);
        audit.UpdateDetails(request.Title, request.Description, request.Scope, request.Location, _currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return audit.ToDetailDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var audit = await _auditRepository.GetTrackedByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("Audit", id);
        audit.SoftDelete(_currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuditTeamMemberDto> AssignTeamMemberAsync(Guid auditId, AssignTeamMemberRequest request, CancellationToken cancellationToken = default)
    {
        // Needs TeamMembers loaded: AssignTeamMember checks for an existing
        // (userId, role) pair, which would silently no-op if the collection
        // weren't actually populated.
        var audit = await _auditRepository.GetTrackedWithDetailsByIdAsync(auditId, cancellationToken) ?? throw NotFoundException.For("Audit", auditId);

        // Shape (is it one of the known role names) is already checked by
        // AssignTeamMemberRequestValidator; parsing here is safe.
        Enum.TryParse<AuditTeamRole>(request.Role, ignoreCase: true, out var role);

        var member = audit.AssignTeamMember(request.UserId, request.DisplayName, role, _currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return member.ToDto();
    }

    public async Task RemoveTeamMemberAsync(Guid auditId, Guid teamMemberId, CancellationToken cancellationToken = default)
    {
        var audit = await _auditRepository.GetTrackedWithDetailsByIdAsync(auditId, cancellationToken) ?? throw NotFoundException.For("Audit", auditId);
        audit.RemoveTeamMember(teamMemberId, _currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuditDetailDto> AssignChecklistAsync(Guid auditId, AssignChecklistRequest request, CancellationToken cancellationToken = default)
    {
        var audit = await _auditRepository.GetTrackedByIdAsync(auditId, cancellationToken) ?? throw NotFoundException.For("Audit", auditId);
        _ = await _checklistRepository.GetByIdAsync(request.ChecklistId, cancellationToken) ?? throw NotFoundException.For("Checklist", request.ChecklistId);

        audit.AssignChecklist(request.ChecklistId, _currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return audit.ToDetailDto();
    }

    public async Task<ChecklistResponseDto> RecordChecklistResponseAsync(Guid auditId, RecordChecklistResponseRequest request, CancellationToken cancellationToken = default)
    {
        // Needs ChecklistResponses loaded: RecordChecklistResponse looks for
        // an existing response to the same question to update in place.
        var audit = await _auditRepository.GetTrackedWithDetailsByIdAsync(auditId, cancellationToken) ?? throw NotFoundException.For("Audit", auditId);

        if (audit.ChecklistId is not { } checklistId)
        {
            throw new ConflictException("Audit has no checklist assigned.", "NO_CHECKLIST_ASSIGNED");
        }

        var checklist = await _checklistRepository.GetByIdAsync(checklistId, cancellationToken) ?? throw NotFoundException.For("Checklist", checklistId);
        if (checklist.Questions.All(q => q.Id != request.ChecklistQuestionId))
        {
            throw new BusinessValidationException(new Dictionary<string, string[]>
            {
                ["checklistQuestionId"] = ["This question does not belong to the audit's assigned checklist."],
            });
        }

        var response = audit.RecordChecklistResponse(request.ChecklistQuestionId, request.AnswerText, request.IsCompliant, _currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return response.ToDto();
    }

    public async Task<IReadOnlyCollection<AuditStatusHistoryDto>> GetStatusHistoryAsync(Guid auditId, CancellationToken cancellationToken = default)
    {
        var audit = await _auditRepository.GetDetailAsync(auditId, cancellationToken) ?? throw NotFoundException.For("Audit", auditId);
        return audit.StatusHistory.OrderBy(h => h.ChangedAtUtc).Select(h => h.ToDto()).ToList();
    }

    public async Task<AuditDetailDto> PlanAsync(Guid auditId, PlanAuditRequest request, CancellationToken cancellationToken = default)
    {
        // Needs TeamMembers loaded: Plan() requires at least one assigned
        // team member (CLAUDE.md §2).
        var audit = await _auditRepository.GetTrackedWithDetailsByIdAsync(auditId, cancellationToken) ?? throw NotFoundException.For("Audit", auditId);
        audit.Plan(request.PlannedStartDate, request.PlannedEndDate, _currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return audit.ToDetailDto();
    }

    public async Task<AuditDetailDto> StartAsync(Guid auditId, CancellationToken cancellationToken = default)
    {
        var audit = await _auditRepository.GetTrackedByIdAsync(auditId, cancellationToken) ?? throw NotFoundException.For("Audit", auditId);
        audit.Start(_currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return audit.ToDetailDto();
    }

    public async Task<AuditDetailDto> CompleteAsync(Guid auditId, CancellationToken cancellationToken = default)
    {
        // Needs ChecklistResponses loaded: Complete() checks every mandatory
        // question has a recorded answer.
        var audit = await _auditRepository.GetTrackedWithDetailsByIdAsync(auditId, cancellationToken) ?? throw NotFoundException.For("Audit", auditId);

        IReadOnlyCollection<Guid> mandatoryQuestionIds = [];
        if (audit.ChecklistId is { } checklistId)
        {
            var checklist = await _checklistRepository.GetByIdAsync(checklistId, cancellationToken);
            mandatoryQuestionIds = checklist?.MandatoryQuestionIds() ?? [];
        }

        audit.Complete(mandatoryQuestionIds, _currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return audit.ToDetailDto();
    }

    public async Task<AuditDetailDto> CloseAsync(Guid auditId, CancellationToken cancellationToken = default)
    {
        var audit = await _auditRepository.GetTrackedByIdAsync(auditId, cancellationToken) ?? throw NotFoundException.For("Audit", auditId);
        var compliance = await _complianceGateway.GetComplianceStatusAsync(auditId, cancellationToken);
        audit.Close(compliance.HasOpenCriticalFindings, compliance.HasOpenRequiredActions, _currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return audit.ToDetailDto();
    }

    public async Task<AuditDetailDto> CancelAsync(Guid auditId, CancelAuditRequest request, CancellationToken cancellationToken = default)
    {
        var audit = await _auditRepository.GetTrackedByIdAsync(auditId, cancellationToken) ?? throw NotFoundException.For("Audit", auditId);
        audit.Cancel(request.Reason, _currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return audit.ToDetailDto();
    }
}
