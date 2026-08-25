namespace Ehs.SharedKernel.Exceptions;

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message, string errorCode = "FORBIDDEN")
        : base(errorCode, message)
    {
    }
}
