using Audit.Application.Validators;
using Audit.Contracts.Requests;
using FluentAssertions;

namespace Audit.UnitTests.Validators;

public class CreateAuditRequestValidatorTests
{
    private readonly CreateAuditRequestValidator _validator = new();

    [Fact]
    public void Valid_request_passes()
    {
        var request = new CreateAuditRequest("Title", "Description", "Scope", "Location");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_title_fails_with_a_field_specific_error()
    {
        var request = new CreateAuditRequest("", "Description", "Scope", "Location");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CreateAuditRequest.Title));
    }

    [Fact]
    public void Title_over_max_length_fails()
    {
        var request = new CreateAuditRequest(new string('x', 201), "Description", "Scope", "Location");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }
}
