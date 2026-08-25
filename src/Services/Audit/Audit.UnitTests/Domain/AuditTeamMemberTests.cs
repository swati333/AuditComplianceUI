using Audit.Domain.Enums;
using Ehs.SharedKernel.Exceptions;
using FluentAssertions;
using AuditEntity = Audit.Domain.Entities.Audit;

namespace Audit.UnitTests.Domain;

public class AuditTeamMemberTests
{
    private const string Actor = "coordinator@example.com";

    private static AuditEntity CreateAudit() =>
        AuditEntity.Create("Audit", null, "Scope", "Location", Actor);

    [Fact]
    public void AssignTeamMember_adds_an_auditor()
    {
        var audit = CreateAudit();

        var member = audit.AssignTeamMember("user-1", "Alice", AuditTeamRole.Auditor, Actor);

        audit.TeamMembers.Should().ContainSingle().Which.Should().BeSameAs(member);
        member.Role.Should().Be(AuditTeamRole.Auditor);
    }

    [Fact]
    public void AssignTeamMember_allows_the_same_user_as_both_auditor_and_auditee()
    {
        var audit = CreateAudit();

        audit.AssignTeamMember("user-1", "Alice", AuditTeamRole.Auditor, Actor);
        audit.AssignTeamMember("user-1", "Alice", AuditTeamRole.Auditee, Actor);

        audit.TeamMembers.Should().HaveCount(2);
    }

    [Fact]
    public void AssignTeamMember_rejects_the_exact_same_user_and_role_twice()
    {
        var audit = CreateAudit();
        audit.AssignTeamMember("user-1", "Alice", AuditTeamRole.Auditor, Actor);

        var act = () => audit.AssignTeamMember("user-1", "Alice", AuditTeamRole.Auditor, Actor);

        act.Should().Throw<ConflictException>().Where(e => e.ErrorCode == "TEAM_MEMBER_ALREADY_ASSIGNED");
    }

    [Fact]
    public void RemoveTeamMember_removes_an_existing_member()
    {
        var audit = CreateAudit();
        var member = audit.AssignTeamMember("user-1", "Alice", AuditTeamRole.Auditor, Actor);

        audit.RemoveTeamMember(member.Id, Actor);

        audit.TeamMembers.Should().BeEmpty();
    }

    [Fact]
    public void RemoveTeamMember_throws_NotFound_for_an_unknown_id()
    {
        var audit = CreateAudit();

        var act = () => audit.RemoveTeamMember(Guid.NewGuid(), Actor);

        act.Should().Throw<NotFoundException>();
    }
}
