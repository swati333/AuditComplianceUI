using Finding.Application.Common;
using Finding.Application.Mapping;
using Finding.Contracts.Dtos;
using Finding.Contracts.Requests;
using Finding.Domain.Enums;
using Ehs.SharedKernel.Exceptions;
using Ehs.SharedKernel.Pagination;
using FindingEntity = Finding.Domain.Entities.Finding;

namespace Finding.Application.Services;

/// <summary>
/// Application-layer use cases for the Finding aggregate. A plain service
/// class rather than MediatR/CQRS handlers (CLAUDE.md §4). Which repository
/// read to use per command matters: any aggregate method that inspects an
/// owned collection needs that collection actually loaded into the tracked
/// instance (see Audit Service's AuditService for the same lesson learned).
/// </summary>
public sealed class FindingService
{
    private readonly IFindingRepository _findingRepository;
    private readonly IAuditReferenceRepository _auditReferenceRepository;
    private readonly IActionPlanGateway _actionPlanGateway;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public FindingService(
        IFindingRepository findingRepository,
        IAuditReferenceRepository auditReferenceRepository,
        IActionPlanGateway actionPlanGateway,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser)
    {
        _findingRepository = findingRepository;
        _auditReferenceRepository = auditReferenceRepository;
        _actionPlanGateway = actionPlanGateway;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<FindingDetailDto> CreateAsync(CreateFindingRequest request, CancellationToken cancellationToken = default)
    {
        // Shape (is it a known severity name) is already checked by
        // CreateFindingRequestValidator; parsing here is safe.
        Enum.TryParse<FindingSeverity>(request.Severity, ignoreCase: true, out var severity);

        // Fail closed only on what we positively know (a locally-cached
        // reference marked Closed); fail open if we haven't seen the audit's
        // events yet, since AuditCreated may simply not have arrived yet in
        // an eventually-consistent system — see AuditReference's doc comment.
        var auditReference = await _auditReferenceRepository.GetByIdAsync(request.AuditId, cancellationToken);
        if (auditReference is { IsClosed: true })
        {
            throw new ConflictException("Cannot create a finding for a closed audit.", "AUDIT_CLOSED");
        }

        var finding = FindingEntity.Create(request.AuditId, request.Title, request.Description, severity, _currentUser.UserId);
        _findingRepository.Add(finding);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return finding.ToDetailDto();
    }

    public async Task<FindingDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var finding = await _findingRepository.GetDetailAsync(id, cancellationToken) ?? throw NotFoundException.For("Finding", id);
        return finding.ToDetailDto();
    }

    public async Task<PagedResult<FindingSummaryDto>> SearchAsync(FindingListQuery query, CancellationToken cancellationToken = default)
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

        var (items, totalCount) = await _findingRepository.SearchAsync(query, cancellationToken);
        var dtos = items.Select(f => f.ToSummaryDto()).ToList();
        return new PagedResult<FindingSummaryDto>(dtos, paging.PageNumber, paging.PageSize, totalCount);
    }

    public async Task<FindingDetailDto> UpdateAsync(Guid id, UpdateFindingRequest request, CancellationToken cancellationToken = default)
    {
        Enum.TryParse<FindingSeverity>(request.Severity, ignoreCase: true, out var severity);

        var finding = await _findingRepository.GetTrackedByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("Finding", id);
        _findingRepository.SetRowVersion(finding, request.RowVersion);
        finding.UpdateDetails(request.Title, request.Description, severity, _currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return finding.ToDetailDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var finding = await _findingRepository.GetTrackedByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("Finding", id);
        finding.SoftDelete(_currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<FindingDetailDto> RecordRootCauseAnalysisAsync(Guid id, RecordRootCauseAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        var finding = await _findingRepository.GetTrackedByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("Finding", id);
        finding.RecordRootCauseAnalysis(request.Text, _currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return finding.ToDetailDto();
    }

    public async Task<FindingCommentDto> AddCommentAsync(Guid id, AddCommentRequest request, CancellationToken cancellationToken = default)
    {
        var finding = await _findingRepository.GetTrackedWithDetailsByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("Finding", id);
        var comment = finding.AddComment(request.AuthorId, request.AuthorName, request.Text, _currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return comment.ToDto();
    }

    public async Task<FindingDocumentDto> AddDocumentAsync(Guid id, AddDocumentRequest request, CancellationToken cancellationToken = default)
    {
        var finding = await _findingRepository.GetTrackedWithDetailsByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("Finding", id);
        var document = finding.AddDocument(request.FileName, request.BlobReference, request.ContentType, request.SizeBytes, _currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return document.ToDto();
    }

    public async Task<IReadOnlyCollection<FindingStatusHistoryDto>> GetStatusHistoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var finding = await _findingRepository.GetDetailAsync(id, cancellationToken) ?? throw NotFoundException.For("Finding", id);
        return finding.StatusHistory.OrderBy(h => h.ChangedAtUtc).Select(h => h.ToDto()).ToList();
    }

    public async Task<FindingDetailDto> StartReviewAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var finding = await _findingRepository.GetTrackedByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("Finding", id);
        finding.StartReview(_currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return finding.ToDetailDto();
    }

    public async Task<FindingDetailDto> RequireActionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var finding = await _findingRepository.GetTrackedByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("Finding", id);
        finding.RequireAction(_currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return finding.ToDetailDto();
    }

    public async Task<FindingDetailDto> ResolveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var finding = await _findingRepository.GetTrackedByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("Finding", id);
        var hasCorrectiveAction = await _actionPlanGateway.HasCorrectiveActionAsync(id, cancellationToken);
        finding.Resolve(hasCorrectiveAction, _currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return finding.ToDetailDto();
    }

    public async Task<FindingDetailDto> VerifyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var finding = await _findingRepository.GetTrackedByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("Finding", id);
        finding.Verify(_currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return finding.ToDetailDto();
    }

    public async Task<FindingDetailDto> CloseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var finding = await _findingRepository.GetTrackedByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("Finding", id);
        finding.Close(_currentUser.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return finding.ToDetailDto();
    }
}
