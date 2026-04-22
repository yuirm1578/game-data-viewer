using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameDataViewer.Core.Contracts;
using GameDataViewer.Core.Models;
using GameDataViewer.Infrastructure.Services;
using Microsoft.Win32;

namespace GameDataViewer.App.ViewModels;

public partial class SkillListViewModel : ViewModelBase
{
    private readonly ISkillRepository  _repo;
    private readonly IExportService     _exportService;
    private readonly AppSettingsService _settings;

    [ObservableProperty] private ObservableCollection<Skill> _skills = [];
    [ObservableProperty] private Skill? _selectedSkill;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _selectedClass = string.Empty;
    [ObservableProperty] private string _selectedType = string.Empty;
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _totalCount;
    [ObservableProperty] private int _totalPages = 1;
    [ObservableProperty] private ObservableCollection<string> _classes = [];
    [ObservableProperty] private ObservableCollection<string> _types = [];
    [ObservableProperty] private string _sortColumn = "Id";
    [ObservableProperty] private bool _sortDescending;

    public SkillListViewModel(
        ISkillRepository   repo,
        IExportService     exportService,
        AppSettingsService settings)
    {
        _repo          = repo;
        _exportService = exportService;
        _settings      = settings;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        SetBusy(true, "스킬 로딩...");
        ClearError();
        try
        {
            var classTask = _repo.GetClassesAsync();
            var typeTask = _repo.GetTypesAsync();
            await Task.WhenAll(classTask, typeTask);
            Classes = ["전체", .. classTask.Result];
            Types = ["전체", .. typeTask.Result];
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            SetError($"스킬 로드 실패: {ex.Message}");
        }
        finally { SetBusy(false); }
    }

    [RelayCommand]
    private async Task SearchAsync() { CurrentPage = 1; await RefreshAsync(); }

    private bool CanNextPage() => CurrentPage < TotalPages;
    private bool CanPrevPage() => CurrentPage > 1;

    [RelayCommand(CanExecute = nameof(CanNextPage))]
    private async Task NextPageAsync() { CurrentPage++; await RefreshAsync(); }

    [RelayCommand(CanExecute = nameof(CanPrevPage))]
    private async Task PrevPageAsync() { CurrentPage--; await RefreshAsync(); }

    partial void OnCurrentPageChanged(int value)
    {
        NextPageCommand.NotifyCanExecuteChanged();
        PrevPageCommand.NotifyCanExecuteChanged();
    }

    partial void OnTotalPagesChanged(int value)
    {
        NextPageCommand.NotifyCanExecuteChanged();
        PrevPageCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    public async Task SortByAsync(string column)
    {
        SortDescending = SortColumn == column ? !SortDescending : false;
        SortColumn = column;
        CurrentPage = 1;
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        var req = new PageRequest
        {
            Page = CurrentPage,
            PageSize = _settings.Current.DefaultPageSize,
            SearchTerm = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
            Grade = SelectedClass is "" or "전체" ? null : SelectedClass,
            Category = SelectedType is "" or "전체" ? null : SelectedType,
            SortColumn = SortColumn,
            SortDescending = SortDescending
        };
        var result = await _repo.GetPagedAsync(req);
        Skills = new ObservableCollection<Skill>(result.Items);
        TotalCount = result.TotalCount;
        TotalPages = result.TotalPages;
        StatusMessage = $"스킬 {TotalCount:#,0}건";
    }

    /// <summary>탭 전환 시 설정 변경(페이지 크기 등)을 즉시 반영</summary>
    public Task ApplySettingsAsync() => RefreshAsync();

    partial void OnSearchTextChanged(string value)
        => Debounce(() => { CurrentPage = 1; _ = RefreshAsync(); });

    partial void OnSelectedClassChanged(string value) => _ = RefreshAsync();
    partial void OnSelectedTypeChanged(string value) => _ = RefreshAsync();

    partial void OnSkillsChanged(ObservableCollection<Skill> value)
        => OnPropertyChanged(nameof(IsEmpty));

    public bool IsEmpty => !IsBusy && (Skills?.Count ?? 0) == 0;

    [RelayCommand]
    private async Task ExportCsvAsync()
    {
        var dlg = new SaveFileDialog
        {
            Title            = "스킬 목록 CSV 내보내기",
            Filter           = "CSV 파일 (*.csv)|*.csv",
            FileName         = $"skills_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
            InitialDirectory = GetExportDirectory(),
        };
        if (dlg.ShowDialog() != true) return;

        SetBusy(true, "CSV 내보내는 중...");
        try
        {
            var data = await GetAllFilteredAsync();
            await _exportService.ExportToCsvAsync(data, dlg.FileName);
            StatusMessage = $"CSV 완료 ({data.Count:#,0}건) — {Path.GetFileName(dlg.FileName)}";
        }
        catch (Exception ex) { StatusMessage = $"내보내기 오류: {ex.Message}"; }
        finally { SetBusy(false); }
    }

    [RelayCommand]
    private async Task ExportExcelAsync()
    {
        var dlg = new SaveFileDialog
        {
            Title            = "스킬 목록 Excel 내보내기",
            Filter           = "Excel 파일 (*.xlsx)|*.xlsx",
            FileName         = $"skills_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
            InitialDirectory = GetExportDirectory(),
        };
        if (dlg.ShowDialog() != true) return;

        SetBusy(true, "Excel 내보내는 중...");
        try
        {
            var data = await GetAllFilteredAsync();
            await _exportService.ExportToExcelAsync(data, dlg.FileName, "스킬");
            StatusMessage = $"Excel 완료 ({data.Count:#,0}건) — {Path.GetFileName(dlg.FileName)}";
        }
        catch (Exception ex) { StatusMessage = $"내보내기 오류: {ex.Message}"; }
        finally { SetBusy(false); }
    }

    private async Task<List<Skill>> GetAllFilteredAsync()
    {
        var req = new PageRequest
        {
            Page = 1, PageSize = 10_000,
            SearchTerm     = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
            Grade          = SelectedClass is "" or "전체" ? null : SelectedClass,
            Category       = SelectedType  is "" or "전체" ? null : SelectedType,
            SortColumn     = SortColumn,
            SortDescending = SortDescending,
        };
        var result = await _repo.GetPagedAsync(req);
        return [.. result.Items];
    }

    private string GetExportDirectory()
    {
        var dir = _settings.Current.ExportDirectory;
        return string.IsNullOrEmpty(dir)
            ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            : dir;
    }
}
