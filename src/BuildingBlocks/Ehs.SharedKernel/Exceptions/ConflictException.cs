namespace Ehs.SharedKernel.Exceptions;

/// <summary>
/// Signals a state/version conflict (e.g. invalid status transition, or a
/// RowVersion mismatch translated by Infrastructure). Maps to HTTP 409/412.
/// </summary>
public sealed class ConflictException : AppException
{
    public ConflictException(string message, string errorCode = "RESOURCE_CONFLICT")
        : base(errorCode, message)
    {
    }
}
