using Dapper;
using GameDataViewer.Core.Contracts;
using GameDataViewer.Core.Models;
using GameDataViewer.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace GameDataViewer.Infrastructure.Repositories;

public class MonsterRepository : IMonsterRepository
{
    private readonly DatabaseContext _db;
    private readonly ILogger<MonsterRepository> _logger;

    public MonsterRepository(DatabaseContext db, ILogger<MonsterRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PageResult<Monster>> GetPagedAsync(PageRequest req, CancellationToken ct = default)
    {
        _logger.LogDebug("GetPaged Monsters: page={Page}, search={Search}, grade={Grade}", req.Page, req.SearchTerm, req.Grade);
        using var conn = _db.CreateConnection();
        var (where, param) = BuildWhereClause(req);

        var total = await conn.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM Monsters {where}", param);

        var orderBy = BuildOrderBy(req);
        var offset = (req.Page - 1) * req.PageSize;
        param.Add("PageSize", req.PageSize);
        param.Add("Offset", offset);
        var items = (await conn.QueryAsync<Monster>(
            $"SELECT * FROM Monsters {where} {orderBy} LIMIT @PageSize OFFSET @Offset", param)).AsList();

        return new PageResult<Monster> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize };
    }

    public async Task<Monster?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Monster>("SELECT * FROM Monsters WHERE Id = @Id", new { Id = id });
    }

    public async Task<IReadOnlyList<string>> GetGradesAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        return (await conn.QueryAsync<string>("SELECT DISTINCT Grade FROM Monsters ORDER BY Grade")).AsList();
    }

    public async Task<IReadOnlyList<string>> GetLocationsAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        return (await conn.QueryAsync<string>("SELECT DISTINCT Location FROM Monsters ORDER BY Location")).AsList();
    }

    public async Task<int> GetTotalCountAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Monsters");
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
            conditions.Add("Location = @Location");
            p.Add("Location", req.Category);
        }
        return (conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : string.Empty, p);
    }

    private static string BuildOrderBy(PageRequest req)
    {
        var col = req.SortColumn switch
        {
            nameof(Monster.Name) => "Name",
            nameof(Monster.Grade) => "Grade",
            nameof(Monster.Level) => "Level",
            nameof(Monster.Hp) => "Hp",
            nameof(Monster.AttackMax) => "AttackMax",
            nameof(Monster.ExpReward) => "ExpReward",
            _ => "Id"
        };
        return $"ORDER BY {col} {(req.SortDescending ? "DESC" : "ASC")}";
    }
}
