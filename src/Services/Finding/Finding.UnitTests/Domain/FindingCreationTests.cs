using Ehs.SharedKernel.Exceptions;
using FluentAssertions;
using Finding.Domain.Enums;
using FindingEntity = Finding.Domain.Entities.Finding;

namespace Finding.UnitTests.Domain;

public class FindingCreationTests
{
    private const string Actor = "auditor@example.com";

    [Fact]
    public void Create_sets_Open_status_and_stamps_created_by()
    {
        var finding = FindingEntity.Create(Guid.NewGuid(), "Blocked exit", "desc", FindingSeverity.High, Actor);

        finding.Status.Should().Be(FindingStatus.Open);
        finding.CreatedBy.Should().Be(Actor);
        finding.Severity.Should().Be(FindingSeverity.High);
    }

    [Fact]
    public void Create_records_initial_status_history_entry()
    {
        var finding = FindingEntity.Create(Guid.NewGuid(), "Title", null, FindingSeverity.Low, Actor);

        var entry = Assert.Single(finding.StatusHistory);
        entry.FromStatus.Should().BeNull();
        entry.ToStatus.Should().Be(FindingStatus.Open);
    }

    [Fact]
    public void Create_raises_only_FindingCreated_for_non_critical_severity()
    {
        var finding = FindingEntity.Create(Guid.NewGuid(), "Title", null, FindingSeverity.Medium, Actor);

        finding.DomainEvents.Should().ContainSingle();
        finding.DomainEvents.Should().ContainSingle(e => e is Finding.Domain.Events.FindingCreatedDomainEvent);
    }

    [Fact]
    public void Create_raises_both_FindingCreated_and_CriticalFindingCreated_for_Critical_severity()
    {
        var finding = FindingEntity.Create(Guid.NewGuid(), "Title", null, FindingSeverity.Critical, Actor);

        finding.DomainEvents.Should().HaveCount(2);
        finding.DomainEvents.Should().ContainSingle(e => e is Finding.Domain.Events.FindingCreatedDomainEvent);
        finding.DomainEvents.Should().ContainSingle(e => e is Finding.Domain.Events.CriticalFindingCreatedDomainEvent);
    }

    [Fact]
    public void Create_rejects_an_empty_title()
    {
        var act = () => FindingEntity.Create(Guid.NewGuid(), "", null, FindingSeverity.Low, Actor);

        act.Should().Throw<BusinessValidationException>();
    }
}
