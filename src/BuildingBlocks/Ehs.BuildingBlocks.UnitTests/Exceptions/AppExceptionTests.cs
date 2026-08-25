using Ehs.SharedKernel.Exceptions;
using Xunit;

namespace Ehs.BuildingBlocks.UnitTests.Exceptions;

public class AppExceptionTests
{
    [Fact]
    public void NotFoundException_For_builds_a_readable_message()
    {
        var exception = NotFoundException.For("Audit", 123);

        Assert.Equal("Audit '123' was not found.", exception.Message);
        Assert.Equal("RESOURCE_NOT_FOUND", exception.ErrorCode);
    }

    [Fact]
    public void ConflictException_defaults_error_code()
    {
        var exception = new ConflictException("Audit cannot close while findings are open.");

        Assert.Equal("RESOURCE_CONFLICT", exception.ErrorCode);
    }

    [Fact]
    public void ForbiddenException_defaults_error_code()
    {
        var exception = new ForbiddenException("Action owner cannot approve their own action.");

        Assert.Equal("FORBIDDEN", exception.ErrorCode);
    }

    [Fact]
    public void BusinessValidationException_carries_field_errors()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["Scope"] = ["Scope is required."],
        };

        var exception = new BusinessValidationException(errors);

        Assert.Equal("VALIDATION_ERROR", exception.ErrorCode);
        Assert.Same(errors, exception.Errors);
    }

    [Fact]
    public void Custom_error_codes_override_the_default()
    {
        var exception = new ConflictException("Overdue action.", errorCode: "ACTION_OVERDUE");

        Assert.Equal("ACTION_OVERDUE", exception.ErrorCode);
    }
}
