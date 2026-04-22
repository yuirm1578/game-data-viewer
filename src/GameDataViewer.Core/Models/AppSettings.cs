namespace GameDataViewer.Core.Models;

/// <summary>앱 전역 설정 (Day 6 설정 화면에서 바인딩)</summary>
public class AppSettings
{
    public int DefaultPageSize { get; set; } = 50;
    public string Theme { get; set; } = "Dark.Blue";
    public string ExportDirectory { get; set; } = string.Empty;
    public bool EnableDebugLogging { get; set; } = false;
    public int SearchDebounceMs { get; set; } = 300;
    public string? CustomDbPath { get; set; }
}
