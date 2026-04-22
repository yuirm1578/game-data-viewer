# Game Data Viewer

리니지M 스타일 게임 데이터(아이템·스킬·몬스터)를 조회·필터링·시각화하는 WPF 데스크톱 애플리케이션입니다.  
.NET 8 + WPF + MVVM (CommunityToolkit.Mvvm) + SQLite 기반의 실전형 포트폴리오 프로젝트로,  
11일 개발 스프린트를 통해 설계부터 릴리즈 패키징까지 전체 워크플로를 완성했습니다.

---

## 주요 기능

| 기능 | 설명 | 상태 |
|------|------|------|
| 아이템 / 스킬 / 몬스터 DataGrid | 등급별 행 색상, 컬럼 정렬 | ✅ |
| 이름 검색 + 다중 콤보박스 필터 | 검색 디바운스 300ms | ✅ |
| 선택 항목 상세 사이드 패널 | 등급·속성 컬러 뱃지 포함 | ✅ |
| 서버 사이드 페이지 네비게이션 | 이전 / 다음, 페이지 정보 표시 | ✅ |
| 서버 사이드 정렬 | DataGrid 헤더 클릭 → SQL ORDER BY | ✅ |
| 등급 분포 파이 차트 | 아이템·몬스터 각 1개 (OxyPlot) | ✅ |
| 수치 비교 수평 바 차트 | 스킬 최대 데미지 TOP 10, 몬스터 HP TOP 10 | ✅ |
| CSV / Excel 내보내기 | 현재 필터 전체 적용, 저장 경로 선택 | ✅ |
| 테마 실시간 전환 | MahApps.Metro 15개 테마 즉시 미리보기 | ✅ |
| 설정 영속화 | settings.json (페이지 크기, 테마, 내보내기 폴더) | ✅ |
| 로딩 오버레이 | BusyOverlay (ProgressRing + 상태 메시지) | ✅ |
| 빈 데이터 EmptyState | "검색 결과가 없습니다." 안내 뷰 | ✅ |
| 전역 예외 처리 | Dispatcher / AppDomain / TaskScheduler 3종 핸들러 | ✅ |
| 접근성 | AutomationProperties.Name 주요 컨트롤 | ✅ |
| Serilog 파일 로그 | 일별 롤링, ILogger<T> 전체 주입 | ✅ |
| 단일 실행 파일 배포 | Self-contained win-x64 EXE (73.6 MB) | ✅ |

---

## 기술 스택

| 레이어 | 기술 | 버전 |
|--------|------|------|
| UI 프레임워크 | WPF (.NET 8) | net8.0-windows |
| UI 테마 | MahApps.Metro (Dark.Blue 기본) | 2.4.10 |
| MVVM | CommunityToolkit.Mvvm | 8.3.2 |
| 차트 | OxyPlot.Wpf | 2.2.0 |
| DB | SQLite (WAL 모드) + Dapper | Microsoft.Data.Sqlite 8.0.11 / Dapper 2.1.35 |
| DI | Microsoft.Extensions.DependencyInjection | 8.0.1 |
| 로깅 | Serilog (File Sink, 일별 롤링) | Sinks.File 5.0.0 |
| 내보내기 | CsvHelper + ClosedXML | 33.0.1 / 0.102.3 |
| HTTP 클라이언트 | Microsoft.Extensions.Http (API 연동용) | 8.0.1 |
| 테스트 (단위) | xUnit + Moq + FluentAssertions | 2.5.3 / 4.20.72 / 6.12.1 |
| 테스트 (E2E) | FlaUI.UIA3 (UI 자동화) | 4.0.0 |
| 빌드 | .NET 8 SDK | 8.0+ |

---

## 아키텍처

```
GameDataViewer.sln
├── src/
│   ├── GameDataViewer.Core             # 도메인 모델 + 인터페이스 (의존성 없음)
│   │   ├── Models/                     # Item, Skill, Monster, PageResult<T>, PageRequest, AppSettings
│   │   └── Contracts/                  # IRepositories.cs (IItemRepository, ISkillRepository, IMonsterRepository)
│   │                                   # IChartDataService, IExportService, IGameApiClient
│   ├── GameDataViewer.Infrastructure   # SQLite + HTTP 구현체 (net8.0)
│   │   ├── ApiClients/                 # LineageMApiClient (실제 API 연동 시 사용)
│   │   ├── Data/                       # DatabaseContext (WAL), DatabaseInitializer, DataSeeder
│   │   ├── Repositories/               # ItemRepository, SkillRepository, MonsterRepository, ChartDataService
│   │   └── Services/                   # AppSettingsService (settings.json), ExportService (CSV/Excel)
│   └── GameDataViewer.App              # WPF 프레젠테이션 레이어 (net8.0-windows)
│       ├── ViewModels/                 # MainViewModel, ItemListViewModel, SkillListViewModel,
│       │                              # MonsterListViewModel, ChartViewModel, SettingsViewModel, ViewModelBase
│       ├── Views/                      # ItemDetailView, SkillDetailView, MonsterDetailView,
│       │                              # ChartView, SettingsView, BusyOverlay
│       ├── Converters/                 # GradeToColorConverter, ElementToColorConverter, BoolToYesNoConverter,
│       │                              # NullToVisibilityConverter, BoolToVisibilityConverter
│       ├── App.xaml / App.xaml.cs      # DI 부트스트랩 + 전역 예외 핸들러 (Dispatcher/AppDomain/TaskScheduler)
│       └── MainWindow.xaml             # 5탭 메인 UI (ListBox 탭 헤더 + TabControl 콘텐츠 분리 구조)
├── tests/
│   ├── GameDataViewer.Tests            # xUnit 단위 테스트 30개
│   │   ├── Repositories/
│   │   │   ├── ItemRepositoryTests.cs  # 검색·필터·정렬·페이징 (12건)
│   │   │   └── PageResultTests.cs      # 페이징 계산 경계값 (6건)
│   │   └── Services/
│   │       └── ExportServiceTests.cs   # CSV/Excel 파일 생성 (12건)
│   └── GameDataViewer.UITests          # FlaUI E2E UI 자동화 테스트
│       ├── UITestFixture.cs            # 앱 생명주기 픽스처 (ICollectionFixture — 컬렉션당 1회 실행)
│       ├── AppTestBase.cs              # 공통 베이스 (SelectTabByName 헬퍼)
│       ├── AppLaunchTests.cs           # 앱 실행 기본 검증 (3건)
│       ├── ItemTabTests.cs             # 아이템 탭 DataGrid·검색 (2건)
│       ├── SkillTabTests.cs            # 스킬 탭 DataGrid·검색·필터 (3건)
│       └── MonsterTabTests.cs          # 몬스터 탭 DataGrid·검색·필터 (3건)
├── data/
│   ├── items.json                      # 리니지M 스타일 아이템 시드 (30건)
│   ├── skills.json                     # 스킬 시드 (25건)
│   └── monsters.json                   # 몬스터 시드 (20건)
└── docs/
    ├── devlog.md                       # 스프린트 개발 일지 (Day 1~12)
    ├── user-guide.md                   # 사용자 가이드
    └── screenshots/                    # 앱 스크린샷 보관 폴더
```

### 레이어 의존 방향

```
App  →  Infrastructure  →  Core
         (implements)
```

- **Core**: 순수 C# 클래스, 외부 의존 없음
- **Infrastructure**: Core 인터페이스 구현, DB/파일 I/O 담당
- **App**: DI로 Infrastructure를 주입받아 MVVM으로 바인딩

---

## 화면 구성

| 탭 | 설명 |
|-----|------|
| ⚔ 아이템 | 검색·등급·카테고리 필터 → DataGrid + 상세 사이드 패널, CSV/XLS 내보내기 |
| ✨ 스킬 | 검색·직업·유형 필터 → DataGrid + 상세 패널, CSV/XLS 내보내기 |
| 👾 몬스터 | 검색·등급·지역 필터 → DataGrid + 상세 패널, CSV/XLS 내보내기 |
| 📊 통계 | 아이템 등급 분포 파이 · 몬스터 등급 분포 파이 · 스킬 데미지 TOP10 바 · 몬스터 HP TOP10 바 |
| ⚙ 설정 | 페이지 크기, 테마 (실시간 미리보기), 내보내기 폴더, 디버그 로그 토글 |

---

## 실행 방법

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows 10 이상 (WPF 필요)
- Visual Studio 2022 17.8+ 또는 VS Code (C# Dev Kit 확장)

### 소스에서 빌드 · 실행

```bash
# 솔루션 전체 빌드
dotnet build GameDataViewer.sln -c Debug

# 앱 실행 (Visual Studio에서 F5 또는)
publish\win-x64\GameDataViewer.exe

# 테스트 실행
dotnet test --logger "console;verbosity=normal"
```

Visual Studio에서는 `GameDataViewer.App`을 시작 프로젝트로 설정 후 **F5**.

> ⚠️ `dotnet run`은 WDAC 정책 환경에서 차단될 수 있습니다.  
> 이 경우 `dotnet publish` 후 EXE를 직접 실행하세요.

### 테스트 실행

```bash
# 단위 테스트 (30개, 빠름)
dotnet test tests/GameDataViewer.Tests

# E2E UI 자동화 테스트 (11개 — publish 후 실행)
dotnet publish src/GameDataViewer.App/GameDataViewer.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/win-x64
dotnet test tests/GameDataViewer.UITests
```

### 실행 파일 배포 (단일 EXE)

```bash
dotnet publish src/GameDataViewer.App/GameDataViewer.App.csproj ^
  -c Release -r win-x64 --self-contained true ^
  -p:PublishSingleFile=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true ^
  -o publish/win-x64
```

출력: `publish/win-x64/GameDataViewer.exe` (약 74 MB, .NET 런타임 내장)

### 첫 실행 시 자동 생성 경로

| 항목 | 경로 |
|------|------|
| DB 파일 | `%LOCALAPPDATA%\GameDataViewer\gamedata.db` |
| 설정 파일 | `%LOCALAPPDATA%\GameDataViewer\settings.json` |
| 로그 파일 | `%LOCALAPPDATA%\GameDataViewer\logs\app-YYYY-MM-DD.log` |

---

## 데이터 출처

- 리니지M 공개 데이터를 기반으로 재구성한 **샘플 데이터**입니다.
- `data/*.json` 파일은 학습 목적으로 작성된 가상 수치이며, 실제 서비스 데이터와 다릅니다.
- 상업적 사용 금지. 개인 포트폴리오 및 학습 용도로만 사용하십시오.

---

## 11일 개발 일정

| Day | 내용 | 상태 |
|-----|------|------|
| **Day 1** | 솔루션 구조, 도메인 모델, Repository 인터페이스, 기초 DI·MVVM·UI | ✅ 완료 |
| **Day 2** | ILogger 통합, DataSeeder Upsert, ExportService, AppSettingsService, 테스트 30개 | ✅ 완료 |
| **Day 3** | Value Converters 4종, Detail UserControls 3종, 행 하이라이트, 서버 사이드 정렬 | ✅ 완료 |
| **Day 4** | OxyPlot 차트 4개 (파이 2 + 바 2), ChartViewModel, 📊 통계 탭 | ✅ 완료 |
| **Day 5** | CSV/XLS 내보내기 UI 연결, SettingsViewModel, SettingsView, ⚙ 설정 탭 | ✅ 완료 |
| **Day 6** | BusyOverlay, EmptyState, 전역 예외 핸들러 3종, 접근성 개선 | ✅ 완료 |
| **Day 7** | 릴리즈 빌드, 단일 EXE 패키징 (73.6 MB), README 최종화 | ✅ 완료 |
| **Day 8** | 팀장 QA 피드백 반영, 사용설명서 작성, HTTP API 레이어 추가, E2E 테스트 프로젝트 구축 | ✅ 완료 |
| **Day 9** | 탭 헤더 구조 개선, 페이징 CanExecute 구현, 설정 즉시 반영 연동 | ✅ 완료 |
| **Day 10** | 최대화 시작, MinHeight 적용으로 탭 바 미표시 근본 해결 | ✅ 완료 |
| **Day 11** | SkillTabTests/MonsterTabTests 추가, ICollectionFixture 앱 공유 적용 | ✅ 완료 |
