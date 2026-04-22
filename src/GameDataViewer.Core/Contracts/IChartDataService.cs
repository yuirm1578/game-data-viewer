using GameDataViewer.Core.Models;

namespace GameDataViewer.Core.Contracts;

/// <summary>차트용 집계 데이터 서비스</summary>
public interface IChartDataService
{
    Task<IReadOnlyList<GradeCount>> GetItemGradeDistributionAsync(CancellationToken ct = default);
    Task<IReadOnlyList<GradeCount>> GetMonsterGradeDistributionAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SkillDamageEntry>> GetTopSkillsByMaxDamageAsync(int topN = 10, CancellationToken ct = default);
    Task<IReadOnlyList<MonsterStatEntry>> GetTopMonstersByHpAsync(int topN = 10, CancellationToken ct = default);
}

public record GradeCount(string Grade, int Count);
public record SkillDamageEntry(string Name, string Class, int MinDamage, int MaxDamage);
public record MonsterStatEntry(string Name, string Grade, int Hp, int Attack);
