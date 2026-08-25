using Audit.Domain.Enums;
using Ehs.SharedKernel.Exceptions;
using FluentAssertions;
using AuditEntity = Audit.Domain.Entities.Audit;

namespace Audit.UnitTests.Domain;

public class AuditCreationTests
{
    [Fact]
    public void Create_sets_Draft_status_and_stamps_created_by()
    {
        var audit = AuditEntity.Create("Fire Safety Audit", "desc", "Plant scope", "Plant A", "alice@example.com");

        audit.Status.Should().Be(AuditStatus.Draft);
        audit.CreatedBy.Should().Be("alice@example.com");
        audit.Title.Should().Be("Fire Safety Audit");
    }

    [Fact]
    public void Create_records_initial_status_history_entry()
    {
        var audit = AuditEntity.Create("Fire Safety Audit", null, "Scope", "Location", "alice@example.com");

        var entry = Assert.Single(audit.StatusHistory);
        entry.FromStatus.Should().BeNull();
        entry.ToStatus.Should().Be(AuditStatus.Draft);
    }

    [Fact]
    public void Create_raises_AuditCreatedDomainEvent()
    {
        var audit = AuditEntity.Create("Fire Safety Audit", null, "Scope", "Location", "alice@example.com");

        var domainEvent = Assert.Single(audit.DomainEvents);
        domainEvent.Should().BeOfType<Audit.Domain.Events.AuditCreatedDomainEvent>();
    }

    [Theory]
    [InlineData("", "Scope", "Location")]
    [InlineData("Title", "", "Location")]
    [InlineData("Title", "Scope", "")]
    public void Create_rejects_missing_required_fields(string title, string scope, string location)
    {
        var act = () => AuditEntity.Create(title, null, scope, location, "alice@example.com");

        act.Should().Throw<BusinessValidationException>();
    }
}
