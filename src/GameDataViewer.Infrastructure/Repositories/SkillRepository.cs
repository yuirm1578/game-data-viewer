using Dapper;
using GameDataViewer.Core.Contracts;
using GameDataViewer.Core.Models;
using GameDataViewer.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace GameDataViewer.Infrastructure.Repositories;

public class SkillRepository : ISkillRepository
{
    private readonly DatabaseContext _db;
    private readonly ILogger<SkillRepository> _logger;

    public SkillRepository(DatabaseContext db, ILogger<SkillRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PageResult<Skill>> GetPagedAsync(PageRequest req, CancellationToken ct = default)
    {
        _logger.LogDebug("GetPaged Skills: page={Page}, search={Search}", req.Page, req.SearchTerm);
        using var conn = _db.CreateConnection();
        var (where, param) = BuildWhereClause(req);

        var total = await conn.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM Skills {where}", param);

        var orderBy = BuildOrderBy(req);
        var offset = (req.Page - 1) * req.PageSize;
        param.Add("PageSize", req.PageSize);
        param.Add("Offset", offset);
        var items = (await conn.QueryAsync<Skill>(
            $"SELECT * FROM Skills {where} {orderBy} LIMIT @PageSize OFFSET @Offset", param)).AsList();

        return new PageResult<Skill> { Items = items, TotalCount = total, Page = req.Page, PageSize = req.PageSize };
    }

    public async Task<Skill?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Skill>("SELECT * FROM Skills WHERE Id = @Id", new { Id = id });
    }

    public async Task<IReadOnlyList<string>> GetClassesAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        return (await conn.QueryAsync<string>("SELECT DISTINCT Class FROM Skills ORDER BY Class")).AsList();
    }

    public async Task<IReadOnlyList<string>> GetTypesAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        return (await conn.QueryAsync<string>("SELECT DISTINCT Type FROM Skills ORDER BY Type")).AsList();
    }

    public async Task<int> GetTotalCountAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Skills");
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
        if (!string.IsNullOrWhiteSpace(req.Grade))   // Grade 필드는 Class 로 매핑
        {
            conditions.Add("Class = @Class");
            p.Add("Class", req.Grade);
        }
        if (!string.IsNullOrWhiteSpace(req.Category)) // Category 필드는 Type 으로 매핑
        {
            conditions.Add("Type = @Type");
            p.Add("Type", req.Category);
        }
        return (conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : string.Empty, p);
    }

    private static string BuildOrderBy(PageRequest req)
    {
        var col = req.SortColumn switch
        {
            nameof(Skill.Name) => "Name",
            nameof(Skill.Class) => "Class",
            nameof(Skill.MaxDamage) => "MaxDamage",
            nameof(Skill.MpCost) => "MpCost",
            nameof(Skill.CooldownSeconds) => "CooldownSeconds",
            _ => "Id"
        };
        return $"ORDER BY {col} {(req.SortDescending ? "DESC" : "ASC")}";
    }
}
