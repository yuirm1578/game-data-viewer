using System.IO;
using System.Windows;
using GameDataViewer.App.ViewModels;
using GameDataViewer.Core.Contracts;
using GameDataViewer.Infrastructure.ApiClients;
using GameDataViewer.Infrastructure.Data;
using GameDataViewer.Infrastructure.Repositories;
using GameDataViewer.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace GameDataViewer.App;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        // ── 전역 예외 핸들러 ──────────────────────────────────────────
        DispatcherUnhandledException += (_, ex) =>
        {
            Log.Error(ex.Exception, "Unhandled dispatcher exception");
            ex.Handled = true;
            System.Windows.MessageBox.Show(
                $"예기치 않은 오류가 발생했습니다.\n\n{ex.Exception.Message}",
                "오류",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        };
        AppDomain.CurrentDomain.UnhandledException += (_, ex) =>
            Log.Fatal(ex.ExceptionObject as Exception, "Unhandled AppDomain exception");
        TaskScheduler.UnobservedTaskException += (_, ex) =>
        {
            Log.Warning(ex.Exception, "Unobserved task exception");
            ex.SetObserved();
        };
        // Serilog 설정 (파일 롤링)
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "GameDataViewer", "logs", "app-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        // DI 컨테이너 구성
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        // DB 초기화 및 시드
        var initializer = Services.GetRequiredService<DatabaseInitializer>();
        initializer.Initialize();

        var seeder = Services.GetRequiredService<DataSeeder>();
        await seeder.SeedAsync();

        // 메인 윈도우 표시
        var mainWindow = Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        // 데이터 로드
        var mainVm = Services.GetRequiredService<MainViewModel>();
        await mainVm.LoadAllCommand.ExecuteAsync(null);

        // 초기 로드 완료 표시 → 이후 탭 전환 시 새 페이지 크기 즉시 반영
        mainVm.InitialLoadCompleted = true;
        mainWindow.Activate();

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // WAL 체크포인트 후 로그 닫기
        try { Services.GetRequiredService<DatabaseContext>().Checkpoint(); } catch { /* ignore */ }
        Log.CloseAndFlush();
        base.OnExit(e);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Logging: Serilog → Microsoft.Extensions.Logging
        services.AddLogging(lb =>
        {
            lb.ClearProviders();
            lb.AddSerilog(Log.Logger, dispose: false);
        });

        // Infrastructure
        services.AddSingleton<DatabaseContext>();
        services.AddSingleton<DatabaseInitializer>();
        services.AddSingleton<DataSeeder>();
        services.AddSingleton<IItemRepository, ItemRepository>();
        services.AddSingleton<ISkillRepository, SkillRepository>();
        services.AddSingleton<IMonsterRepository, MonsterRepository>();
        services.AddSingleton<IChartDataService, ChartDataService>();
        services.AddSingleton<IExportService, ExportService>();
        services.AddSingleton<AppSettingsService>();

        // ── 실제 API 연동 시 아래 주석을 해제하고 DataSeeder 호출을 제거하세요 ──────────
        // services.AddHttpClient<IGameApiClient, LineageMApiClient>(client =>
        // {
        //     client.BaseAddress = new Uri(LineageMApiClient.BaseAddress);
        //     // TODO: API Key 헤더 추가 (예: client.DefaultRequestHeaders.Add("X-Api-Key", apiKey))
        // });
        // ────────────────────────────────────────────────────────────────────────────────

        // ViewModels
        services.AddSingleton<ItemListViewModel>();
        services.AddSingleton<SkillListViewModel>();
        services.AddSingleton<MonsterListViewModel>();
        services.AddSingleton<ChartViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<MainViewModel>();

        // Views
        services.AddSingleton<MainWindow>();
    }
}


