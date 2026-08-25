using Audit.Application.Common;
using Audit.Application.Mapping;
using Audit.Contracts.Dtos;
using Audit.Contracts.Requests;
using Audit.Domain.Entities;
using Ehs.SharedKernel.Exceptions;

namespace Audit.Application.Services;

public sealed class ChecklistService
{
    private readonly IChecklistRepository _checklistRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ChecklistService(IChecklistRepository checklistRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _checklistRepository = checklistRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<ChecklistDto> CreateAsync(CreateChecklistRequest request, CancellationToken cancellationToken = default)
    {
        var checklist = Checklist.Create(request.Name, request.Description, _currentUser.UserId);
        foreach (var question in request.Questions)
        {
            checklist.AddQuestion(question.Text, question.IsMandatory, question.DisplayOrder, _currentUser.UserId);
        }

        _checklistRepository.Add(checklist);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return checklist.ToDto();
    }

    public async Task<ChecklistDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var checklist = await _checklistRepository.GetByIdAsync(id, cancellationToken) ?? throw NotFoundException.For("Checklist", id);
        return checklist.ToDto();
    }

    public async Task<IReadOnlyCollection<ChecklistDto>> ListActiveAsync(CancellationToken cancellationToken = default)
    {
        var checklists = await _checklistRepository.ListActiveAsync(cancellationToken);
        return checklists.Select(c => c.ToDto()).ToList();
    }
}
