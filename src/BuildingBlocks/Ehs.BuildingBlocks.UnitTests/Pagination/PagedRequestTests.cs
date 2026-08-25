using Ehs.SharedKernel.Pagination;
using Xunit;

namespace Ehs.BuildingBlocks.UnitTests.Pagination;

public class PagedRequestTests
{
    [Fact]
    public void PageNumber_defaults_to_one()
    {
        var request = new PagedRequest();

        Assert.Equal(1, request.PageNumber);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(3, 3)]
    public void PageNumber_clamps_below_one(int input, int expected)
    {
        var request = new PagedRequest { PageNumber = input };

        Assert.Equal(expected, request.PageNumber);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-10, 1)]
    [InlineData(50, 50)]
    [InlineData(101, 100)]
    [InlineData(1000, 100)]
    public void PageSize_is_clamped_between_one_and_max(int input, int expected)
    {
        var request = new PagedRequest { PageSize = input };

        Assert.Equal(expected, request.PageSize);
    }

    [Fact]
    public void PageSize_defaults_to_twenty()
    {
        var request = new PagedRequest();

        Assert.Equal(20, request.PageSize);
    }
}
