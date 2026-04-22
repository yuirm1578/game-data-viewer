using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Theming;
using GameDataViewer.Core.Models;
using GameDataViewer.Infrastructure.Services;

namespace GameDataViewer.App.ViewModels;

/// <summary>설정 탭 VM — AppSettings 편집·저장·테마 적용</summary>
public partial class SettingsViewModel : ViewModelBase
{
    private readonly AppSettingsService _settingsService;

    [ObservableProperty] private int    _defaultPageSize   = 50;
    [ObservableProperty] private string _theme             = "Dark.Blue";
    [ObservableProperty] private string _exportDirectory   = string.Empty;
    [ObservableProperty] private bool   _enableDebugLogging;
    [ObservableProperty] private int    _searchDebounceMs  = 300;

    public IReadOnlyList<string> AvailableThemes { get; } =
    [
        "Dark.Blue",   "Dark.Green",  "Dark.Red",    "Dark.Purple",
        "Dark.Orange", "Dark.Teal",   "Dark.Cyan",   "Dark.Amber",
        "Dark.Indigo", "Dark.Crimson",
        "Light.Blue",  "Light.Green", "Light.Red",   "Light.Purple", "Light.Orange",
    ];

    public SettingsViewModel(AppSettingsService settingsService)
    {
        _settingsService = settingsService;
        LoadFromService();
    }

    // ── 명령 ────────────────────────────────────────────────────────────────

    [RelayCommand]
    private void Save()
    {
        _settingsService.Save(new AppSettings
        {
            DefaultPageSize    = DefaultPageSize,
            Theme              = Theme,
            ExportDirectory    = ExportDirectory,
            EnableDebugLogging = EnableDebugLogging,
            SearchDebounceMs   = SearchDebounceMs,
        });
        StatusMessage = "✅ 설정이 저장되었습니다.";
    }

    [RelayCommand]
    private void Reset()
    {
        _settingsService.Save(new AppSettings());
        LoadFromService();
        StatusMessage = "↩ 기본값으로 초기화했습니다.";
    }

    [RelayCommand]
    private void BrowseDirectory()
    {
        var dlg = new Microsoft.Win32.OpenFolderDialog
        {
            Title            = "기본 내보내기 폴더 선택",
            InitialDirectory = string.IsNullOrEmpty(ExportDirectory)
                               ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                               : ExportDirectory,
        };
        if (dlg.ShowDialog() == true)
            ExportDirectory = dlg.FolderName;
    }

    // ── 테마 실시간 미리보기 ─────────────────────────────────────────────────
    partial void OnThemeChanged(string value)
    {
        if (string.IsNullOrEmpty(value)) return;
        try { ThemeManager.Current.ChangeTheme(Application.Current, value); }
        catch { /* 잘못된 테마명 무시 */ }
    }

    // ── 내부 헬퍼 ───────────────────────────────────────────────────────────
    private void LoadFromService()
    {
        var s             = _settingsService.Current;
        DefaultPageSize   = s.DefaultPageSize;
        Theme             = s.Theme;
        ExportDirectory   = s.ExportDirectory;
        EnableDebugLogging = s.EnableDebugLogging;
        SearchDebounceMs  = s.SearchDebounceMs;
    }
}
