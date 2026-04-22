namespace GameDataViewer.Core.Models;

/// <summary>리니지M 아이템 모델</summary>
public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;        // 등급 (일반/희귀/영웅/전설)
    public string Category { get; set; } = string.Empty;     // 무기/방어구/장신구 등
    public string SubCategory { get; set; } = string.Empty;  // 세부 분류
    public int AttackPower { get; set; }
    public int DefensePower { get; set; }
    public int MagicPower { get; set; }
    public int RequiredLevel { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsTradable { get; set; }
    public long Price { get; set; }
}
