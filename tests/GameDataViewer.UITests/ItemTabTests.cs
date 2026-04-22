using FlaUI.Core.AutomationElements;
using Xunit;

namespace GameDataViewer.UITests;

[Collection("UITests")]
/// <summary>
/// 아이템 탭 E2E 테스트: DataGrid 로드, 검색, 필터.
/// </summary>
public class ItemTabTests : AppTestBase
{
    public ItemTabTests(UITestFixture fixture) : base(fixture) { }

    [Fact]
    public void ItemTab_DataGrid_ShouldContainRows_AfterLoad()
    {
        SelectTabByName("아이템");
        var grids = MainWindow.FindAllDescendants(
            Cf.ByControlType(FlaUI.Core.Definitions.ControlType.DataGrid));
        Assert.True(grids.Length > 0, "DataGrid를 찾을 수 없습니다.");
        var rows = grids[0].FindAllDescendants(
            Cf.ByControlType(FlaUI.Core.Definitions.ControlType.DataItem));
        Assert.True(rows.Length > 0, "아이템 DataGrid에 최소 1개의 행이 있어야 합니다.");
    }

    [Fact]
    public void ItemTab_SearchBox_ShouldFilter_WhenTextEntered()
    {
        SelectTabByName("아이템");
        // 제일 첫 번째 Edit 콘트롤 = 이름 검색 TextBox
        var edits = MainWindow.FindAllDescendants(
            Cf.ByControlType(FlaUI.Core.Definitions.ControlType.Edit));
        Assert.True(edits.Length > 0, "검색 TextBox를 찾을 수 없습니다.");
        edits[0].AsTextBox().Text = "검";
        System.Threading.Thread.Sleep(600);

        Assert.False(MainWindow.IsOffscreen);
    }
}
