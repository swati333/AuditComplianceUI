namespace Ehs.SharedKernel.Exceptions;

/// <summary>
/// Base type for exceptions that are expected to cross the Application/Domain
/// boundary and be translated into an RFC 7807 ProblemDetails response by the
/// Api layer's global exception handler (Ehs.Observability). Deliberately has
/// no dependency on ASP.NET Core so it can be thrown from Domain/Application.
/// </summary>
public abstract class AppException : Exception
{
    protected AppException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    protected AppException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

    public string ErrorCode { get; }
}
