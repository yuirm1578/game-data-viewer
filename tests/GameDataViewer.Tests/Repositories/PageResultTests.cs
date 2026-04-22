using FluentAssertions;
using GameDataViewer.Core.Models;

namespace GameDataViewer.Tests.Repositories;

/// <summary>PageResult<T> 계산 로직 단위 테스트</summary>
public class PageResultTests
{
    [Theory]
    [InlineData(100, 10, 10)]
    [InlineData(101, 10, 11)]
    [InlineData(50,  50, 1)]
    [InlineData(0,   10, 0)]
    public void TotalPages_IsCalculatedCorrectly(int totalCount, int pageSize, int expectedPages)
    {
        var result = new PageResult<Item>
        {
            Items = [],
            TotalCount = totalCount,
            Page = 1,
            PageSize = pageSize
        };

        result.TotalPages.Should().Be(expectedPages);
    }

    [Fact]
    public void HasPreviousPage_IsFalseOnFirstPage()
    {
        var result = new PageResult<Item> { Items = [], TotalCount = 100, Page = 1, PageSize = 10 };
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void HasNextPage_IsFalseOnLastPage()
    {
        var result = new PageResult<Item> { Items = [], TotalCount = 100, Page = 10, PageSize = 10 };
        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void HasNextPage_IsTrueWhenNotOnLastPage()
    {
        var result = new PageResult<Item> { Items = [], TotalCount = 100, Page = 1, PageSize = 10 };
        result.HasNextPage.Should().BeTrue();
    }
}
