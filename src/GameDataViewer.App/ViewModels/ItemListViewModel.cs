using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameDataViewer.Core.Contracts;
using GameDataViewer.Core.Models;
using GameDataViewer.Infrastructure.Services;
using Microsoft.Win32;

namespace GameDataViewer.App.ViewModels;

public partial class ItemListViewModel : ViewModelBase
{
    private readonly IItemRepository    _repo;
    private readonly IExportService     _exportService;
    private readonly AppSettingsService _settings;

    [ObservableProperty] private ObservableCollection<Item> _items = [];
    [ObservableProperty] private Item? _selectedItem;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _selectedGrade = string.Empty;
    [ObservableProperty] private string _selectedCategory = string.Empty;
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _totalCount;
    [ObservableProperty] private int _totalPages = 1;
    [ObservableProperty] private ObservableCollection<string> _grades = [];
    [ObservableProperty] private ObservableCollection<string> _categories = [];
    [ObservableProperty] private string _sortColumn = "Id";
    [ObservableProperty] private bool _sortDescending;

    public ItemListViewModel(
        IItemRepository    repo,
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
        SetBusy(true, "아이템 로딩...");
        ClearError();
        try
        {
            var gradesTask = _repo.GetGradesAsync();
            var catsTask = _repo.GetCategoriesAsync();
            await Task.WhenAll(gradesTask, catsTask);

            Grades = ["전체", .. gradesTask.Result];
            Categories = ["전체", .. catsTask.Result];
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            SetError($"아이템 로드 실패: {ex.Message}");
        }
        finally { SetBusy(false); }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        CurrentPage = 1;
        await RefreshAsync();
    }

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
            Grade = SelectedGrade is "" or "전체" ? null : SelectedGrade,
            Category = SelectedCategory is "" or "전체" ? null : SelectedCategory,
            SortColumn = SortColumn,
            SortDescending = SortDescending
        };
        var result = await _repo.GetPagedAsync(req);
        Items = new ObservableCollection<Item>(result.Items);
        TotalCount = result.TotalCount;
        TotalPages = result.TotalPages;
        StatusMessage = $"아이템 {TotalCount:#,0}건 / {CurrentPage}/{TotalPages} 페이지";
    }

    /// <summary>탭 전환 시 설정 변경(페이지 크기 등)을 즉시 반영</summary>
    public Task ApplySettingsAsync() => RefreshAsync();

    partial void OnSearchTextChanged(string value)
        => Debounce(() => { CurrentPage = 1; _ = RefreshAsync(); });

    partial void OnSelectedGradeChanged(string value) => _ = RefreshAsync();
    partial void OnSelectedCategoryChanged(string value) => _ = RefreshAsync();

    partial void OnItemsChanged(ObservableCollection<Item> value)
        => OnPropertyChanged(nameof(IsEmpty));

    public bool IsEmpty => !IsBusy && (Items?.Count ?? 0) == 0;

    [RelayCommand]
    private async Task ExportCsvAsync()
    {
        var dlg = new SaveFileDialog
        {
            Title            = "아이템 목록 CSV 내보내기",
            Filter           = "CSV 파일 (*.csv)|*.csv",
            FileName         = $"items_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
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
            Title            = "아이템 목록 Excel 내보내기",
            Filter           = "Excel 파일 (*.xlsx)|*.xlsx",
            FileName         = $"items_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
            InitialDirectory = GetExportDirectory(),
        };
        if (dlg.ShowDialog() != true) return;

        SetBusy(true, "Excel 내보내는 중...");
        try
        {
            var data = await GetAllFilteredAsync();
            await _exportService.ExportToExcelAsync(data, dlg.FileName, "아이템");
            StatusMessage = $"Excel 완료 ({data.Count:#,0}건) — {Path.GetFileName(dlg.FileName)}";
        }
        catch (Exception ex) { StatusMessage = $"내보내기 오류: {ex.Message}"; }
        finally { SetBusy(false); }
    }

    private async Task<List<Item>> GetAllFilteredAsync()
    {
        var req = new PageRequest
        {
            Page = 1, PageSize = 10_000,
            SearchTerm     = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText,
            Grade          = SelectedGrade    is "" or "전체" ? null : SelectedGrade,
            Category       = SelectedCategory is "" or "전체" ? null : SelectedCategory,
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
