using Dapper;
using GameDataViewer.Core.Contracts;
using GameDataViewer.Core.Models;
using GameDataViewer.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace GameDataViewer.Infrastructure.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly DatabaseContext _db;
    private readonly ILogger<ItemRepository> _logger;

    public ItemRepository(DatabaseContext db, ILogger<ItemRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PageResult<Item>> GetPagedAsync(PageRequest req, CancellationToken ct = default)
    {
        _logger.LogDebug("GetPaged Items: page={Page}, search={Search}, grade={Grade}", req.Page, req.SearchTerm, req.Grade);
        using var conn = _db.CreateConnection();

        var (where, param) = BuildWhereClause(req);

        var countSql = $"SELECT COUNT(*) FROM Items {where}";
        var total = await conn.ExecuteScalarAsync<int>(countSql, param);

        var orderBy = BuildOrderBy(req);
        var offset = (req.Page - 1) * req.PageSize;
        var dataSql = $"SELECT * FROM Items {where} {orderBy} LIMIT @PageSize OFFSET @Offset";
        param.Add("PageSize", req.PageSize);
        param.Add("Offset", offset);

        var items = (await conn.QueryAsync<Item>(dataSql, param)).AsList();
        return new PageResult<Item>
        {
            Items = items,
            TotalCount = total,
            Page = req.Page,
            PageSize = req.PageSize
        };
    }

    public async Task<Item?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Item>("SELECT * FROM Items WHERE Id = @Id", new { Id = id });
    }

    public async Task<IReadOnlyList<string>> GetGradesAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        var grades = await conn.QueryAsync<string>("SELECT DISTINCT Grade FROM Items ORDER BY Grade");
        return grades.AsList();
    }

    public async Task<IReadOnlyList<string>> GetCategoriesAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        var cats = await conn.QueryAsync<string>("SELECT DISTINCT Category FROM Items ORDER BY Category");
        return cats.AsList();
    }

    public async Task<int> GetTotalCountAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Items");
    }

    private static (string sql, DynamicParameters p) BuildWhereClause(PageRequest req)
    {
        var conditions = new List<string>();
        var p = new DynamicParameters();
        if (!string.IsNullOrWhiteSpace(req.SearchTerm))
        {
            conditions.Add("Name LIKE @Search");
            p.Add("Search", $"%{req.SearchTerm}%");
        }
        if (!string.IsNullOrWhiteSpace(req.Grade))
        {
            conditions.Add("Grade = @Grade");
            p.Add("Grade", req.Grade);
        }
        if (!string.IsNullOrWhiteSpace(req.Category))
        {
            conditions.Add("Category = @Category");
            p.Add("Category", req.Category);
        }
        var where = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : string.Empty;
        return (where, p);
    }

    private static string BuildOrderBy(PageRequest req)
    {
        var col = req.SortColumn switch
        {
            nameof(Item.Name) => "Name",
            nameof(Item.Grade) => "Grade",
            nameof(Item.Category) => "Category",
            nameof(Item.AttackPower) => "AttackPower",
            nameof(Item.RequiredLevel) => "RequiredLevel",
            nameof(Item.Price) => "Price",
            _ => "Id"
        };
        return $"ORDER BY {col} {(req.SortDescending ? "DESC" : "ASC")}";
    }
}
