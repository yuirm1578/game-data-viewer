using Dapper;
using GameDataViewer.Core.Contracts;
using GameDataViewer.Infrastructure.Data;

namespace GameDataViewer.Infrastructure.Repositories;

public class ChartDataService : IChartDataService
{
    private readonly DatabaseContext _db;
    public ChartDataService(DatabaseContext db) => _db = db;

    public async Task<IReadOnlyList<GradeCount>> GetItemGradeDistributionAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<(string Grade, int Count)>(
            "SELECT Grade, COUNT(*) AS Count FROM Items GROUP BY Grade ORDER BY Count DESC");
        return rows.Select(r => new GradeCount(r.Grade, r.Count)).ToList();
    }

    public async Task<IReadOnlyList<GradeCount>> GetMonsterGradeDistributionAsync(CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<(string Grade, int Count)>(
            "SELECT Grade, COUNT(*) AS Count FROM Monsters GROUP BY Grade ORDER BY Count DESC");
        return rows.Select(r => new GradeCount(r.Grade, r.Count)).ToList();
    }

    public async Task<IReadOnlyList<SkillDamageEntry>> GetTopSkillsByMaxDamageAsync(int topN = 10, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<(string Name, string Class, int MinDamage, int MaxDamage)>(
            "SELECT Name, Class, MinDamage, MaxDamage FROM Skills ORDER BY MaxDamage DESC LIMIT @TopN", new { TopN = topN });
        return rows.Select(r => new SkillDamageEntry(r.Name, r.Class, r.MinDamage, r.MaxDamage)).ToList();
    }

    public async Task<IReadOnlyList<MonsterStatEntry>> GetTopMonstersByHpAsync(int topN = 10, CancellationToken ct = default)
    {
        using var conn = _db.CreateConnection();
        var rows = await conn.QueryAsync<(string Name, string Grade, int Hp, int Attack)>(
            "SELECT Name, Grade, Hp, AttackMax AS Attack FROM Monsters ORDER BY Hp DESC LIMIT @TopN", new { TopN = topN });
        return rows.Select(r => new MonsterStatEntry(r.Name, r.Grade, r.Hp, r.Attack)).ToList();
    }
}
