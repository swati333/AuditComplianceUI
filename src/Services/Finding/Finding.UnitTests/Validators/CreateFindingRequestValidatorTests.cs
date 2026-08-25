using Finding.Application.Validators;
using Finding.Contracts.Requests;
using FluentAssertions;

namespace Finding.UnitTests.Validators;

public class CreateFindingRequestValidatorTests
{
    private readonly CreateFindingRequestValidator _validator = new();

    [Fact]
    public void Valid_request_passes()
    {
        var request = new CreateFindingRequest(Guid.NewGuid(), "Title", "Description", "High");

        _validator.Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_AuditId_fails()
    {
        var request = new CreateFindingRequest(Guid.Empty, "Title", null, "Low");

        _validator.Validate(request).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("NotASeverity")]
    [InlineData("")]
    public void Unknown_severity_fails(string severity)
    {
        var request = new CreateFindingRequest(Guid.NewGuid(), "Title", null, severity);

        _validator.Validate(request).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("low")]
    [InlineData("CRITICAL")]
    public void Known_severity_passes_case_insensitively(string severity)
    {
        var request = new CreateFindingRequest(Guid.NewGuid(), "Title", null, severity);

        _validator.Validate(request).IsValid.Should().BeTrue();
    }
}
