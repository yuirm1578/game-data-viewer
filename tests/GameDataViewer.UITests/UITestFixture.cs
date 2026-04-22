using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.UIA3;
using Xunit;

namespace GameDataViewer.UITests;

/// <summary>
/// xUnit Collection 정의 — 동일 컬렉션 내 모든 테스트 클래스가 UITestFixture 인스턴스를 공유합니다.
/// 앱은 컬렉션 전체에서 단 한 번 실행되고 단 한 번 종료됩니다.
/// </summary>
[CollectionDefinition("UITests")]
public class UITestCollection : ICollectionFixture<UITestFixture> { }

/// <summary>
/// 앱 생명주기를 관리하는 공유 픽스처.
/// xUnit이 컬렉션당 한 번만 생성·소멸시킵니다.
/// </summary>
public class UITestFixture : IDisposable
{
    private static readonly string AppPath =
        Environment.GetEnvironmentVariable("APP_PATH")
        ?? Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory,
                @"..\..\..\..\..\publish\win-x64\GameDataViewer.exe"));

    public Application App { get; }
    public UIA3Automation Automation { get; }
    public Window MainWindow { get; }
    public ConditionFactory Cf { get; }

    public UITestFixture()
    {
        if (!File.Exists(AppPath))
            throw new FileNotFoundException(
                $"앱 실행 파일을 찾을 수 없습니다. dotnet publish 를 먼저 실행하세요.\n경로: {AppPath}");

        Automation = new UIA3Automation();
        Cf = Automation.ConditionFactory;
        App = Application.Launch(AppPath);
        // 앱 초기 로딩 대기 (DB 시드 + UI 렌더링)
        MainWindow = App.GetMainWindow(Automation, TimeSpan.FromSeconds(15));
        // 데이터 로드 완료 대기 (LoadAllCommand 비동기 실행 + SQLite 시드 완료)
        System.Threading.Thread.Sleep(5000);
    }

    public void Dispose()
    {
        try { App.Close(); } catch { /* 이미 종료됐을 수 있음 */ }
        Automation.Dispose();
    }
}
