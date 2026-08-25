using Audit.Application.Validators;
using Audit.Contracts.Requests;
using FluentAssertions;

namespace Audit.UnitTests.Validators;

public class PlanAuditRequestValidatorTests
{
    private readonly PlanAuditRequestValidator _validator = new();

    [Fact]
    public void End_date_before_start_date_fails()
    {
        var request = new PlanAuditRequest(DateTime.UtcNow, DateTime.UtcNow.AddDays(-1));

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Equal_start_and_end_dates_pass()
    {
        var date = DateTime.UtcNow;
        var request = new PlanAuditRequest(date, date);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}
