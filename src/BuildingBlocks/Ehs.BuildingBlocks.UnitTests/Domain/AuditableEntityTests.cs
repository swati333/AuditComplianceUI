using Ehs.SharedKernel.Domain;
using Xunit;

namespace Ehs.BuildingBlocks.UnitTests.Domain;

public class AuditableEntityTests
{
    private sealed class TestAuditableEntity : AuditableEntity<Guid>
    {
        public TestAuditableEntity(Guid id) => Id = id;
    }

    [Fact]
    public void Defaults_are_not_deleted_and_have_empty_row_version()
    {
        var entity = new TestAuditableEntity(Guid.NewGuid());

        Assert.False(entity.IsDeleted);
        Assert.Empty(entity.RowVersion);
        Assert.Null(entity.ModifiedBy);
        Assert.Null(entity.ModifiedDate);
    }

    [Fact]
    public void Audit_columns_are_settable()
    {
        var entity = new TestAuditableEntity(Guid.NewGuid())
        {
            CreatedBy = "auditor@example.com",
            CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ModifiedBy = "reviewer@example.com",
            ModifiedDate = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            IsDeleted = true,
            RowVersion = [1, 2, 3],
        };

        Assert.Equal("auditor@example.com", entity.CreatedBy);
        Assert.Equal("reviewer@example.com", entity.ModifiedBy);
        Assert.True(entity.IsDeleted);
        Assert.Equal([1, 2, 3], entity.RowVersion);
    }
}
