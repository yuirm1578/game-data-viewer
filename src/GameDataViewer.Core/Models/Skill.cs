namespace GameDataViewer.Core.Models;

/// <summary>리니지M 스킬 모델</summary>
public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;        // 직업 (기사/마법사/요정 등)
    public string Type { get; set; } = string.Empty;         // 액티브/패시브/오라
    public string Element { get; set; } = string.Empty;      // 속성 (화/수/풍/토/무)
    public int MinDamage { get; set; }
    public int MaxDamage { get; set; }
    public int MpCost { get; set; }
    public double CooldownSeconds { get; set; }
    public int Range { get; set; }
    public int MaxLevel { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsUltimate { get; set; }
}
