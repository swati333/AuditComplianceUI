using Audit.Application.Validators;
using Audit.Contracts.Requests;
using FluentAssertions;

namespace Audit.UnitTests.Validators;

public class AssignTeamMemberRequestValidatorTests
{
    private readonly AssignTeamMemberRequestValidator _validator = new();

    [Theory]
    [InlineData("Auditor")]
    [InlineData("auditee")]
    public void Known_roles_pass_case_insensitively(string role)
    {
        var request = new AssignTeamMemberRequest("user-1", "Alice", role);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Unknown_role_fails()
    {
        var request = new AssignTeamMemberRequest("user-1", "Alice", "Approver");

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }
}
