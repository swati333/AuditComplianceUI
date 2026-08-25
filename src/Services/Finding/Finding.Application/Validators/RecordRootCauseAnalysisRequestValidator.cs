using Finding.Contracts.Requests;
using FluentValidation;

namespace Finding.Application.Validators;

public sealed class RecordRootCauseAnalysisRequestValidator : AbstractValidator<RecordRootCauseAnalysisRequest>
{
    public RecordRootCauseAnalysisRequestValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(8000);
    }
}
