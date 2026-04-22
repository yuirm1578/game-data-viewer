namespace GameDataViewer.Core.Models;

/// <summary>리니지M 몬스터 모델</summary>
public class Monster
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;        // 일반/엘리트/보스/레이드
    public string Element { get; set; } = string.Empty;      // 속성
    public string Location { get; set; } = string.Empty;     // 출몰 지역
    public int Level { get; set; }
    public int Hp { get; set; }
    public int AttackMin { get; set; }
    public int AttackMax { get; set; }
    public int Defense { get; set; }
    public int MagicDefense { get; set; }
    public int ExpReward { get; set; }
    public string DropItems { get; set; } = string.Empty;    // JSON 문자열 (간략 표현)
    public string Description { get; set; } = string.Empty;
    public bool IsBoss { get; set; }
}
