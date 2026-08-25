namespace Ehs.SharedKernel.Exceptions;

/// <summary>
/// Carries centralized business/model validation failures (e.g. FluentValidation
/// results) up to the global exception handler. Named to avoid clashing with
/// FluentValidation.ValidationException / System.ComponentModel.DataAnnotations.
/// </summary>
public sealed class BusinessValidationException : AppException
{
    public BusinessValidationException(IReadOnlyDictionary<string, string[]> errors, string errorCode = "VALIDATION_ERROR")
        : base(errorCode, "One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
