using Audit.Domain.Enums;
using Ehs.SharedKernel.Exceptions;
using FluentAssertions;
using AuditEntity = Audit.Domain.Entities.Audit;

namespace Audit.UnitTests.Domain;

public class ChecklistResponseRecordingTests
{
    private const string Actor = "auditor@example.com";

    private static AuditEntity CreateInProgressAudit()
    {
        var audit = AuditEntity.Create("Audit", null, "Scope", "Location", Actor);
        audit.AssignTeamMember("user-1", "Alice", AuditTeamRole.Auditor, Actor);
        audit.Plan(DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(2), Actor);
        audit.Start(Actor);
        return audit;
    }

    [Fact]
    public void RecordChecklistResponse_fails_before_the_audit_is_InProgress()
    {
        var audit = AuditEntity.Create("Audit", null, "Scope", "Location", Actor);

        var act = () => audit.RecordChecklistResponse(Guid.NewGuid(), "Yes", true, Actor);

        act.Should().Throw<ConflictException>().Where(e => e.ErrorCode == "AUDIT_NOT_IN_PROGRESS");
    }

    [Fact]
    public void RecordChecklistResponse_rejects_an_empty_answer()
    {
        var audit = CreateInProgressAudit();

        var act = () => audit.RecordChecklistResponse(Guid.NewGuid(), "   ", null, Actor);

        act.Should().Throw<BusinessValidationException>();
    }

    [Fact]
    public void RecordChecklistResponse_adds_a_new_response()
    {
        var audit = CreateInProgressAudit();
        var questionId = Guid.NewGuid();

        var response = audit.RecordChecklistResponse(questionId, "Compliant", true, Actor);

        audit.ChecklistResponses.Should().ContainSingle().Which.Should().BeSameAs(response);
    }

    [Fact]
    public void RecordChecklistResponse_updates_an_existing_response_in_place_rather_than_duplicating()
    {
        var audit = CreateInProgressAudit();
        var questionId = Guid.NewGuid();
        audit.RecordChecklistResponse(questionId, "Initial answer", false, Actor);

        audit.RecordChecklistResponse(questionId, "Corrected answer", true, Actor);

        var response = Assert.Single(audit.ChecklistResponses);
        response.AnswerText.Should().Be("Corrected answer");
        response.IsCompliant.Should().BeTrue();
    }
}
