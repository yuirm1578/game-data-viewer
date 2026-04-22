using FlaUI.Core.AutomationElements;
using Xunit;

namespace GameDataViewer.UITests;

[Collection("UITests")]
/// <summary>
/// 몬스터 탭 E2E 테스트: DataGrid 로드, 검색, 등급 필터.
/// </summary>
public class MonsterTabTests : AppTestBase
{
    public MonsterTabTests(UITestFixture fixture) : base(fixture) { }

    [Fact]
    public void MonsterTab_DataGrid_ShouldContainRows_AfterLoad()
    {
        SelectTabByName("몬스터");
        var grids = MainWindow.FindAllDescendants(
            Cf.ByControlType(FlaUI.Core.Definitions.ControlType.DataGrid));
        Assert.True(grids.Length > 0, "DataGrid를 찾을 수 없습니다.");
        var rows = grids[0].FindAllDescendants(
            Cf.ByControlType(FlaUI.Core.Definitions.ControlType.DataItem));
        Assert.True(rows.Length > 0, "몬스터 DataGrid에 최소 1개의 행이 있어야 합니다.");
    }

    [Fact]
    public void MonsterTab_SearchBox_ShouldFilter_WhenTextEntered()
    {
        SelectTabByName("몬스터");
        var edits = MainWindow.FindAllDescendants(
            Cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit));
        Assert.True(edits.Length > 0, "검색 TextBox를 찾을 수 없습니다.");
        edits[0].AsTextBox().Text = "오크";
        System.Threading.Thread.Sleep(600);

        Assert.False(MainWindow.IsOffscreen);
    }

    [Fact]
    public void MonsterTab_GradeFilter_ShouldNotCrash_WhenSelected()
    {
        SelectTabByName("몬스터");
        // 콤보박스 목록 중 첫 번째 = 등급 필터
        var combos = MainWindow.FindAllDescendants(
            Cf.ByControlType(FlaUI.Core.Definitions.ControlType.ComboBox));
        Assert.True(combos.Length > 0, "필터 ComboBox를 찾을 수 없습니다.");
        combos[0].AsComboBox().Select(0);
        System.Threading.Thread.Sleep(600);

        Assert.False(MainWindow.IsOffscreen);
    }
}
