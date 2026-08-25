using Ehs.SharedKernel.Domain;
using Xunit;

namespace Ehs.BuildingBlocks.UnitTests.Domain;

public class EntityTests
{
    private sealed class TestEntity : Entity<Guid>
    {
        public TestEntity(Guid id) => Id = id;

        public void RaiseTestEvent() => AddDomainEvent(new TestDomainEvent());
    }

    private sealed record TestDomainEvent : DomainEvent;

    private sealed class OtherEntity : Entity<Guid>
    {
        public OtherEntity(Guid id) => Id = id;
    }

    [Fact]
    public void Entities_with_same_id_and_type_are_equal()
    {
        var id = Guid.NewGuid();

        var a = new TestEntity(id);
        var b = new TestEntity(id);

        Assert.Equal(a, b);
        Assert.True(a == b);
    }

    [Fact]
    public void Entities_with_different_ids_are_not_equal()
    {
        var a = new TestEntity(Guid.NewGuid());
        var b = new TestEntity(Guid.NewGuid());

        Assert.NotEqual(a, b);
        Assert.True(a != b);
    }

    [Fact]
    public void Entities_of_different_types_with_same_id_are_not_equal()
    {
        var id = Guid.NewGuid();

        Entity<Guid> a = new TestEntity(id);
        Entity<Guid> b = new OtherEntity(id);

        Assert.False(a.Equals(b));
    }

    [Fact]
    public void AddDomainEvent_is_visible_via_DomainEvents_until_cleared()
    {
        var entity = new TestEntity(Guid.NewGuid());

        entity.RaiseTestEvent();
        Assert.Single(entity.DomainEvents);

        entity.ClearDomainEvents();
        Assert.Empty(entity.DomainEvents);
    }
}
