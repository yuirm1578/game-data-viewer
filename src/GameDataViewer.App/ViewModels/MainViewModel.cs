using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameDataViewer.App.ViewModels;

namespace GameDataViewer.App.ViewModels;

/// <summary>메인 윈도우 VM — 탭 네비게이션 담당</summary>
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private int _selectedTabIndex;

    [ObservableProperty]
    private string _windowTitle = "Game Data Viewer — 리니지M";

    public ItemListViewModel Items { get; }
    public SkillListViewModel Skills { get; }
    public MonsterListViewModel Monsters { get; }
    public ChartViewModel Charts { get; }
    public SettingsViewModel Settings { get; }

    public MainViewModel(
        ItemListViewModel    items,
        SkillListViewModel   skills,
        MonsterListViewModel monsters,
        ChartViewModel       charts,
        SettingsViewModel    settings)
    {
        Items    = items;
        Skills   = skills;
        Monsters = monsters;
        Charts   = charts;
        Settings = settings;
    }

    [RelayCommand]
    private async Task LoadAllAsync()
    {
        SetBusy(true, "데이터 로딩 중...");
        try
        {
            await Task.WhenAll(
                Items.LoadCommand.ExecuteAsync(null),
                Skills.LoadCommand.ExecuteAsync(null),
                Monsters.LoadCommand.ExecuteAsync(null),
                Charts.LoadCommand.ExecuteAsync(null));
            StatusMessage = $"로드 완료 — 아이템 {Items.TotalCount:#,0} / 스킬 {Skills.TotalCount:#,0} / 몬스터 {Monsters.TotalCount:#,0}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"오류: {ex.Message}";
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>초기 LoadAll 완료 후 true — 탭 전환 시 이중 로드 방지</summary>
    public bool InitialLoadCompleted { get; set; }

    /// <summary>탭 전환 시 변경된 페이지 크기(설정)를 즉시 반영</summary>
    partial void OnSelectedTabIndexChanged(int value)
    {
        if (!InitialLoadCompleted) return;
        _ = value switch
        {
            0 => Items.ApplySettingsAsync(),
            1 => Skills.ApplySettingsAsync(),
            2 => Monsters.ApplySettingsAsync(),
            _ => Task.CompletedTask
        };
    }
}
