using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.UIA3;

namespace GameDataViewer.UITests;

/// <summary>
/// E2E 테스트 공통 베이스 클래스.
/// UITestFixture(앱 생명주기)를 주입받아 모든 테스트에서 앱 인스턴스를 공유합니다.
/// 앱은 컬렉션 전체에서 단 한 번 실행되고 단 한 번 종료됩니다.
/// </summary>
public abstract class AppTestBase
{
    protected Application App { get; }
    protected UIA3Automation Automation { get; }
    protected Window MainWindow { get; }
    protected ConditionFactory Cf { get; }

    protected AppTestBase(UITestFixture fixture)
    {
        App = fixture.App;
        Automation = fixture.Automation;
        MainWindow = fixture.MainWindow;
        Cf = fixture.Cf;
    }

    /// <summary>
    /// 외부 ListBox의 ListBoxItem을 클릭해 탭을 전환합니다.
    /// (탭 헤더가 ListBox로 분리된 구조에 대응)
    /// </summary>
    protected void SelectTabByName(string tabName)
    {
        var items = MainWindow.FindAllDescendants(
            Cf.ByControlType(FlaUI.Core.Definitions.ControlType.ListItem));
        var item = items.FirstOrDefault(i => i.Name.Contains(tabName));
        if (item is null)
            throw new InvalidOperationException($"탭 '{tabName}'을 찾을 수 없습니다.");
        item.Click();
        System.Threading.Thread.Sleep(1200); // 탭 전환 + 쿼리 완료 대기
    }
}
