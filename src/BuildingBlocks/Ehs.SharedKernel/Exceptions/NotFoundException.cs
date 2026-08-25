namespace Ehs.SharedKernel.Exceptions;

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message, string errorCode = "RESOURCE_NOT_FOUND")
        : base(errorCode, message)
    {
    }

    public static NotFoundException For(string resourceName, object key) =>
        new($"{resourceName} '{key}' was not found.");
}
