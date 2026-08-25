using FluentAssertions;
using Finding.Domain.Entities;

namespace Finding.UnitTests.Domain;

public class AuditReferenceTests
{
    [Fact]
    public void Create_defaults_to_not_closed()
    {
        var reference = AuditReference.Create(Guid.NewGuid(), "Q1 Audit");

        reference.IsClosed.Should().BeFalse();
    }

    [Fact]
    public void MarkClosed_sets_IsClosed_and_updates_the_timestamp()
    {
        var reference = AuditReference.Create(Guid.NewGuid(), "Q1 Audit");
        var before = reference.UpdatedAtUtc;

        reference.MarkClosed();

        reference.IsClosed.Should().BeTrue();
        reference.UpdatedAtUtc.Should().BeOnOrAfter(before);
    }
}
