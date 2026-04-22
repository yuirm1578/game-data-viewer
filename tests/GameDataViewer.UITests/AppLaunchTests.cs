using FlaUI.Core.AutomationElements;
using Xunit;

namespace GameDataViewer.UITests;

[Collection("UITests")]
/// <summary>
/// 앱 실행 및 기본 윈도우 검증 E2E 테스트.
/// </summary>
public class AppLaunchTests : AppTestBase
{
    public AppLaunchTests(UITestFixture fixture) : base(fixture) { }

    [Fact]
    public void MainWindow_ShouldBeVisible_AfterLaunch()
    {
        Assert.False(MainWindow.IsOffscreen, "메인 윈도우가 화면에 표시되어야 합니다.");
    }

    [Fact]
    public void MainWindow_Title_ShouldContainAppName()
    {
        Assert.Contains("Game Data Viewer", MainWindow.Title,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TabControl_ShouldHave_FiveTabs()
    {
        // 탭 헤더는 ListBox의 ListBoxItem으로 렌더링됨 (5개: 아이템·스킬·몬스터·통계·설정)
        var listItems = MainWindow.FindAllDescendants(
            Cf.ByControlType(FlaUI.Core.Definitions.ControlType.ListItem));
        Assert.Equal(5, listItems.Length);
    }
}
