using Dapper;
using FluentAssertions;
using GameDataViewer.Core.Models;
using GameDataViewer.Infrastructure.Data;
using GameDataViewer.Infrastructure.Repositories;
using Microsoft.Extensions.Logging.Abstractions;

namespace GameDataViewer.Tests.Repositories;

public class ItemRepositoryTests : IDisposable
{
    private readonly string _dbPath;
    private readonly DatabaseContext _db;
    private readonly ItemRepository _repo;

    public ItemRepositoryTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"GDV_ItemTest_{Guid.NewGuid():N}.db");
        _db = new DatabaseContext(NullLogger<DatabaseContext>.Instance, _dbPath);
        var init = new DatabaseInitializer(_db, NullLogger<DatabaseInitializer>.Instance);
        init.Initialize();
        _repo = new ItemRepository(_db, NullLogger<ItemRepository>.Instance);
        SeedTestData();
    }

    private void SeedTestData()
    {
        using var conn = _db.CreateConnection();
        conn.Execute(@"
INSERT INTO Items (Name,Grade,Category,SubCategory,AttackPower,DefensePower,MagicPower,RequiredLevel,Description,ImageUrl,IsTradable,Price) VALUES
 ('BlazeEdge',  'Hero',    'Weapon','Sword',150,0,0,30,'Fire sword','',1,15000),
 ('IronShield', 'Rare',    'Armor', 'Sub',  0,80,0,20,'Steel shield','',1,8000),
 ('ManaPotion', 'Normal',  'Misc',  'Potion',0,0,0,1,'MP potion'  ,'',1,500),
 ('HighPotion',   'Rare',    'Misc',  'Potion',0,0,0,1,'High MP potion','',1,1200),
 ('LegendStaff','Legendary','Weapon','Staff',0,0,200,50,'Legend staff','',0,99999)
");
    }

    [Fact]
    public async Task GetPagedAsync_NoFilter_ReturnsAllItems()
    {
        var result = await _repo.GetPagedAsync(new PageRequest { Page = 1, PageSize = 10 });
        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(5);
    }

    [Fact]
    public async Task GetPagedAsync_WithSearchTerm_ReturnsFilteredItems()
    {
        var result = await _repo.GetPagedAsync(new PageRequest { Page = 1, PageSize = 10, SearchTerm = "Potion" });
        result.TotalCount.Should().Be(2);
        result.Items.Should().AllSatisfy(i => i.Name.Should().Contain("Potion"));
    }

    [Fact]
    public async Task GetPagedAsync_WithGradeFilter_ReturnsMatchingGradeOnly()
    {
        var result = await _repo.GetPagedAsync(new PageRequest { Page = 1, PageSize = 10, Grade = "Hero" });
        result.TotalCount.Should().Be(1);
        result.Items[0].Name.Should().Be("BlazeEdge");
    }

    [Fact]
    public async Task GetPagedAsync_WithCategoryFilter_ReturnsCategoryItems()
    {
        var result = await _repo.GetPagedAsync(new PageRequest { Page = 1, PageSize = 10, Category = "Misc" });
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetPagedAsync_Pagination_RespectsPageSize()
    {
        var result = await _repo.GetPagedAsync(new PageRequest { Page = 1, PageSize = 2 });
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(3);
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task GetPagedAsync_SecondPage_ReturnsNextItems()
    {
        var p1 = await _repo.GetPagedAsync(new PageRequest { Page = 1, PageSize = 2, SortColumn = "Id" });
        var p2 = await _repo.GetPagedAsync(new PageRequest { Page = 2, PageSize = 2, SortColumn = "Id" });
        p1.Items.Select(i => i.Id).Should().NotIntersectWith(p2.Items.Select(i => i.Id));
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsItem()
    {
        var all = await _repo.GetPagedAsync(new PageRequest { Page = 1, PageSize = 10, SortColumn = "Id" });
        var item = await _repo.GetByIdAsync(all.Items[0].Id);
        item.Should().NotBeNull();
        item!.Name.Should().Be("BlazeEdge");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        (await _repo.GetByIdAsync(9999)).Should().BeNull();
    }

    [Fact]
    public async Task GetGradesAsync_ReturnsDistinctGrades()
    {
        var grades = await _repo.GetGradesAsync();
        grades.Should().OnlyHaveUniqueItems();
        grades.Should().Contain("Hero").And.Contain("Legendary");
    }

    [Fact]
    public async Task GetCategoriesAsync_ReturnsDistinctCategories()
    {
        var cats = await _repo.GetCategoriesAsync();
        cats.Should().OnlyHaveUniqueItems();
        cats.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetTotalCountAsync_ReturnsCorrectCount()
    {
        (await _repo.GetTotalCountAsync()).Should().Be(5);
    }

    [Fact]
    public async Task GetPagedAsync_SortByPrice_Descending_OrdersCorrectly()
    {
        var result = await _repo.GetPagedAsync(
            new PageRequest { Page = 1, PageSize = 10, SortColumn = "Price", SortDescending = true });
        result.Items.Should().BeInDescendingOrder(i => i.Price);
    }

    public void Dispose()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        foreach (var f in new[] { _dbPath, _dbPath + "-wal", _dbPath + "-shm" })
            try { if (File.Exists(f)) File.Delete(f); } catch { }
    }
}