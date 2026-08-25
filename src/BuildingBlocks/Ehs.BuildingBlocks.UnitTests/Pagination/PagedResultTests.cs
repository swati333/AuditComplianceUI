using Ehs.SharedKernel.Pagination;
using Xunit;

namespace Ehs.BuildingBlocks.UnitTests.Pagination;

public class PagedResultTests
{
    [Theory]
    [InlineData(25, 10, 3)]
    [InlineData(20, 10, 2)]
    [InlineData(21, 10, 3)]
    [InlineData(0, 10, 0)]
    public void TotalPages_is_computed_by_ceiling_division(int totalCount, int pageSize, int expectedPages)
    {
        var result = new PagedResult<int>([], pageNumber: 1, pageSize: pageSize, totalCount: totalCount);

        Assert.Equal(expectedPages, result.TotalPages);
    }

    [Fact]
    public void HasPreviousPage_is_false_on_first_page()
    {
        var result = new PagedResult<int>([1, 2], pageNumber: 1, pageSize: 10, totalCount: 15);

        Assert.False(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public void HasNextPage_is_false_on_last_page()
    {
        var result = new PagedResult<int>([1, 2], pageNumber: 2, pageSize: 10, totalCount: 15);

        Assert.True(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public void Empty_returns_zero_items_and_zero_total()
    {
        var result = PagedResult<string>.Empty(pageNumber: 1, pageSize: 20);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
    }

    [Theory]
    [InlineData(0, 10, 5)]
    [InlineData(1, 0, 5)]
    [InlineData(1, 10, -1)]
    public void Constructor_rejects_invalid_arguments(int pageNumber, int pageSize, int totalCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PagedResult<int>([], pageNumber, pageSize, totalCount));
    }
}
