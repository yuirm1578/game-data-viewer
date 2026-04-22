using System.ComponentModel;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using GameDataViewer.App.ViewModels;

namespace GameDataViewer.App;

public partial class MainWindow : MetroWindow
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private MainViewModel Vm => (MainViewModel)DataContext;

    // ── DataGrid 헤더 클릭 정렬 핸들러 ────────────────────────

    private void ItemDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
    {
        e.Handled = true;
        var col = e.Column.SortMemberPath;
        if (string.IsNullOrEmpty(col)) return;
        _ = Vm.Items.SortByCommand.ExecuteAsync(col);
        SetSortIndicator((DataGrid)sender, e.Column, Vm.Items.SortDescending);
    }

    private void SkillDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
    {
        e.Handled = true;
        var col = e.Column.SortMemberPath;
        if (string.IsNullOrEmpty(col)) return;
        _ = Vm.Skills.SortByCommand.ExecuteAsync(col);
        SetSortIndicator((DataGrid)sender, e.Column, Vm.Skills.SortDescending);
    }

    private void MonsterDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
    {
        e.Handled = true;
        var col = e.Column.SortMemberPath;
        if (string.IsNullOrEmpty(col)) return;
        _ = Vm.Monsters.SortByCommand.ExecuteAsync(col);
        SetSortIndicator((DataGrid)sender, e.Column, Vm.Monsters.SortDescending);
    }

    /// <summary>정렬된 컬럼에만 방향 표시 아이콘을 적용하고 나머지는 초기화한다.</summary>
    private static void SetSortIndicator(DataGrid grid, DataGridColumn sorted, bool descending)
    {
        foreach (var c in grid.Columns) c.SortDirection = null;
        sorted.SortDirection = descending
            ? ListSortDirection.Descending
            : ListSortDirection.Ascending;
    }
}
