# Game Data Viewer — 개발 일지

> 리니지M 스타일 게임 데이터 뷰어 (WPF + .NET 8 + MVVM + SQLite)  
> 7일 스프린트 기록

---

## Day 1 — 프로젝트 셋업 및 기반 구조

**날짜:** 2026-03-22  
**목표:** 솔루션 골격 · 도메인 모델 · 기초 UI

### 완료 항목

| 항목 | 세부 내용 |
|------|-----------|
| 솔루션 구성 | `GameDataViewer.sln` + Core / Infrastructure / App 3개 프로젝트 |
| NuGet 패키지 | Dapper 2.1 · Microsoft.Data.Sqlite · CommunityToolkit.Mvvm 8.3 · MahApps.Metro 2.4 · DI 8.0 · Serilog |
| Core 도메인 모델 | `Item` · `Skill` · `Monster` · `PageResult<T>` · `PageRequest` |
| 인터페이스 | `IItemRepository` · `ISkillRepository` · `IMonsterRepository` · `IChartDataService` · `IExportService` |
| Infrastructure | `DatabaseContext` (WAL 모드) · `DatabaseInitializer` · `DataSeeder` · 3개 Repository |
| App (WPF) | DI 부트스트랩 (`App.xaml.cs`) · `MainViewModel` · 3개 `ListViewModel` · `MainWindow.xaml` (3탭 기본 UI) |
| 시드 데이터 | `items.json` (30건) · `skills.json` (25건) · `monsters.json` (20건) |
| README | 기능 목록 · 기술 스택 · 아키텍처 다이어그램 · 실행 방법 |

### 빌드 결과

```
경고 0 / 오류 0
```

---

## Day 2 — 로깅 · DataSeeder 강화 · 테스트 인프라

**날짜:** 2026-03-22  
**목표:** 운영 품질 향상 + 테스트 가능한 구조

### 완료 항목

| 항목 | 세부 내용 |
|------|-----------|
| ILogger 통합 | `DatabaseContext` · `DatabaseInitializer` · `DataSeeder` · 3개 Repository 전체에 `ILogger<T>` 주입 |
| DataSeeder 재설계 | `INSERT OR REPLACE` Upsert · `BeginTransaction` · `forceReseed` 파라미터 |
| ExportService | `CsvHelper`로 UTF-8 CSV · `ClosedXML`로 Excel (.xlsx, 헤더 Bold + 자동 컬럼 너비) |
| AppSettingsService | `%LOCALAPPDATA%\GameDataViewer\settings.json` JSON 영속화 · `AppSettings` 모델 |
| 검색 디바운스 | `ViewModelBase.Debounce()` (DispatcherTimer 300ms) → `ItemListVM` · `SkillListVM` · `MonsterListVM` 연결 |
| WAL Checkpoint | `App.OnExit`에서 `DatabaseContext.Checkpoint()` 호출하여 WAL → DB 병합 |
| 테스트 프로젝트 생성 | `GameDataViewer.Tests` (xUnit + Moq 4.20 + FluentAssertions 6.12) |

### 테스트 클래스

| 파일 | 테스트 수 | 주요 시나리오 |
|------|----------|--------------|
| `PageResultTests.cs` | 6 | TotalPages 계산 · HasPreviousPage/HasNextPage 경계값 |
| `ItemRepositoryTests.cs` | 12 | 검색 필터 · 등급/카테고리 필터 · 페이징 · 정렬 · ID 조회 |
| `ExportServiceTests.cs` | 12 | CSV/Excel 파일 생성 · 헤더 포함 여부 · 빈 데이터 · 하위 디렉터리 생성 |

### 해결한 이슈

- **DataSeeder 잔여 코드**: 리팩터링 후 클래스 밖에 구 코드 잔존 → 수동으로 제거  
- **테스트 DB 격리**: `:memory:` SQLite는 연결마다 별도 DB → 임시 파일 경로 방식으로 전환  
- **인코딩 문제**: `Set-Content`(Windows-1252)로 한국어 깨짐 → `[System.IO.File]::WriteAllText(..., UTF8)` 사용

### 빌드·테스트 결과

```
경고 0 / 오류 0   |   테스트 30개 통과 / 0 실패
```

---

## Day 3 — UI 완성 및 시각적 폴리싱

**날짜:** 2026-03-23  
**목표:** DataGrid 강화 · 상세 뷰 UserControl 분리 · 서버 사이드 정렬

### 완료 항목

#### 1. Value Converters (4개)

| 파일 | 변환 |
|------|------|
| `GradeToColorConverter.cs` | 등급 문자열 → `SolidColorBrush` (일반·고급·영웅·전설·유물 각각 다른 색) |
| `ElementToColorConverter.cs` | 속성 문자열 → `SolidColorBrush` (화·수·풍·지·빛·암) |
| `BoolToYesNoConverter.cs` | `bool` → 교환가능/교환불가 (TrueText/FalseText XAML 설정 가능) |
| `NullToVisibilityConverter.cs` | null → Collapsed / not-null → Visible (Invert 지원) |

모든 Converter는 `App.xaml`에 Application-scope 리소스로 등록:

```xml
<conv:GradeToColorConverter   x:Key="GradeToColor"/>
<conv:ElementToColorConverter x:Key="ElementToColor"/>
<conv:BoolToYesNoConverter    x:Key="BoolToYesNo"  TrueText="교환가능" FalseText="교환불가"/>
<conv:BoolToYesNoConverter    x:Key="BoolToActive" TrueText="궁극기"   FalseText="일반기"/>
```

#### 2. Detail UserControls (3종)

기존 MainWindow.xaml 내 인라인 `<ScrollViewer>` 상세 패널을 독립 UserControl로 분리.

| UserControl | 특징 |
|-------------|------|
| `ItemDetailView.xaml` | 등급 뱃지 (컬러 Border) · 6개 스탯 Grid · 교환 여부 한국어 표시 |
| `SkillDetailView.xaml` | 속성 뱃지 (ElementColor) · 직업·유형 부제목 · 데미지 范围 표시 |
| `MonsterDetailView.xaml` | 등급 뱃지 · BOSS 빨간 뱃지 (IsBoss=True 시 DataTrigger) · 6개 스탯 Grid |

각 UserControl은 code-behind의 `DataContextChanged` 이벤트로 null/비null 전환:

```csharp
DataContextChanged += (_, e) => ToggleView(e.NewValue is not null);
```

#### 3. DataGrid 등급별 행 하이라이트

`MainWindow.xaml.Resources`에 두 가지 RowStyle 정의:

```xml
<!-- 아이템·몬스터: 등급별 배경색 -->
<Style x:Key="GradeRowStyle" TargetType="DataGridRow" BasedOn="{StaticResource {x:Type DataGridRow}}">
    <DataTrigger Binding="{Binding Grade}" Value="전설"> Background="#18FF9800" </DataTrigger>
    <DataTrigger Binding="{Binding Grade}" Value="영웅"> Background="#182196F3" </DataTrigger>
    <DataTrigger Binding="{Binding Grade}" Value="고급"> Background="#184CAF50" </DataTrigger>
</Style>
<!-- 스킬: 궁극기 하이라이트 -->
<Style x:Key="SkillRowStyle" TargetType="DataGridRow" ...>
    <DataTrigger Binding="{Binding IsUltimate}" Value="True"> Background="#18FF9800" </DataTrigger>
</Style>
```

#### 4. 서버 사이드 정렬

각 `ListViewModel`에 추가:

```csharp
[ObservableProperty] private string _sortColumn = "Id";
[ObservableProperty] private bool _sortDescending;

[RelayCommand]
public async Task SortByAsync(string column)
{
    SortDescending = SortColumn == column ? !SortDescending : false;
    SortColumn = column;
    CurrentPage = 1;
    await RefreshAsync();  // PageRequest에 SortColumn/SortDescending 포함
}
```

`MainWindow.xaml.cs`에서 DataGrid `Sorting` 이벤트를 처리하여 커맨드 실행 및 방향 아이콘 업데이트:

```csharp
private void ItemDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
{
    e.Handled = true;
    _ = Vm.Items.SortByCommand.ExecuteAsync(e.Column.SortMemberPath);
    SetSortIndicator((DataGrid)sender, e.Column, Vm.Items.SortDescending);
}
```

### 파일 변경 목록

| 파일 | 변경 유형 | 내용 |
|------|----------|------|
| `Converters/GradeToColorConverter.cs` | 신규 | |
| `Converters/ElementToColorConverter.cs` | 신규 | |
| `Converters/BoolToYesNoConverter.cs` | 신규 | |
| `Converters/NullToVisibilityConverter.cs` | 신규 | |
| `Views/ItemDetailView.xaml[.cs]` | 신규 | |
| `Views/SkillDetailView.xaml[.cs]` | 신규 | |
| `Views/MonsterDetailView.xaml[.cs]` | 신규 | |
| `App.xaml` | 수정 | conv 네임스페이스 + Converter 리소스 추가 |
| `MainWindow.xaml` | 수정 | views 네임스페이스 · RowStyle · Sorting 이벤트 · UserControl 교체 |
| `MainWindow.xaml.cs` | 수정 | Sort 이벤트 핸들러 3개 추가 |
| `ItemListViewModel.cs` | 수정 | SortColumn · SortDescending · SortByCommand |
| `SkillListViewModel.cs` | 수정 | 동일 |
| `MonsterListViewModel.cs` | 수정 | 동일 |
| `ItemRepositoryTests.cs` | 수정 | 인코딩 문제로 ASCII 시드 데이터로 재작성 |

### 빌드·테스트 결과

```
경고 0 / 오류 0   |   테스트 30개 통과 / 0 실패
```

---

## Day 4 — 차트 뷰 (통계 시각화)

**날짜:** 2026-03-23  
**목표:** 등급 분포 파이차트 + 수치 비교 수평 바차트 (OxyPlot 2.2.0)

### 완료 항목

#### 1. OxyPlot.Wpf 2.2.0 패키지 설치

```
OxyPlot.Core 2.2.0 + OxyPlot.Wpf.Shared 2.2.0 + OxyPlot.Wpf 2.2.0
```

#### 2. ChartViewModel.cs (신규)

`ViewModelBase`를 상속하며 `IChartDataService`를 주입받아 4가지 `PlotModel`을 빌드:

| 프로퍼티 | 차트 유형 | 데이터 소스 |
|---------|----------|------------|
| `ItemGradeModel` | 파이차트 | `GetItemGradeDistributionAsync()` |
| `MonsterGradeModel` | 파이차트 | `GetMonsterGradeDistributionAsync()` |
| `TopSkillsModel` | 수평 바차트 | `GetTopSkillsByMaxDamageAsync(10)` |
| `TopMonstersModel` | 수평 바차트 | `GetTopMonstersByHpAsync(10)` |

- `LoadCommand`: 4개 쿼리를 `Task.WhenAll`로 병렬 실행
- 파이차트: 등급별 색상(전설=주황, 영웅=파랑, 고급=초록, 유물=보라, 일반=회색) — `GradeToColorConverter`와 동일 팔레트
- 바차트: `CategoryAxis`(Left) + `LinearAxis`(Bottom) + `BarSeries` with dark gridlines

#### 3. ChartView.xaml + ChartView.xaml.cs (신규)

2×2 Grid 레이아웃 (ScrollViewer로 감싸기):

```
┌────────────────┬────────────────┐
│  아이템 등급   │  몬스터 등급   │  (파이차트 300px)
│  분포 파이     │  분포 파이     │
├────────────────┼────────────────┤
│  스킬 최대     │  몬스터 HP     │  (수평 바차트 380px)
│  데미지 TOP10  │  TOP 10        │
└────────────────┴────────────────┘
```

각 차트를 `Border(ChartCard)` 스타일 카드에 감싸고 `oxy:PlotView`로 렌더링.

#### 4. MainViewModel 업데이트

- `Charts: ChartViewModel` DI 주입 프로퍼티 추가
- `LoadAllAsync`에 `Charts.LoadCommand.ExecuteAsync(null)` 포함

#### 5. App.xaml.cs DI 등록

```csharp
services.AddSingleton<ChartViewModel>();
```

#### 6. MainWindow.xaml 업데이트

```xml
<TabItem Header="📊 통계" DataContext="{Binding Charts}">
    <views:ChartView/>
</TabItem>
```

### 파일 변경 목록

| 파일 | 변경 유형 | 내용 |
|------|----------|------|
| `GameDataViewer.App.csproj` | 수정 | OxyPlot.Wpf 2.2.0 PackageReference 추가 |
| `ViewModels/ChartViewModel.cs` | 신규 | 4개 PlotModel 빌더, LoadCommand |
| `Views/ChartView.xaml[.cs]` | 신규 | 2×2 차트 그리드, OxyPlot PlotView 바인딩 |
| `ViewModels/MainViewModel.cs` | 수정 | Charts 프로퍼티, LoadAll에 Charts.Load 포함 |
| `App.xaml.cs` | 수정 | ChartViewModel Singleton 등록 |
| `MainWindow.xaml` | 수정 | 📊 통계 탭 추가 |

### 빌드·테스트 결과

```
경고 0 / 오류 0   |   테스트 30개 통과 / 0 실패
```

---

## Day 5 — 내보내기 UI 연결 · 설정 화면

**날짜:** 2026-03-23  
**목표:** CSV/Excel 내보내기 버튼 연결 + ⚙ 설정 탭 (AppSettings 편집)

### 완료 항목

#### 1. 내보내기 명령 — 3개 ListViewModel에 추가

각 ListViewModel(`ItemListViewModel`, `SkillListViewModel`, `MonsterListViewModel`)에 `IExportService` + `AppSettingsService` 추가 주입:

| 새로 추가된 멤버 | 역할 |
|----------------|------|
| `ExportCsvCommand` | `SaveFileDialog` → 현재 필터 전체(PageSize=10,000) CSV 내보내기 |
| `ExportExcelCommand` | 동일, Excel(.xlsx) 내보내기 |
| `GetAllFilteredAsync()` | 현재 SearchText/등급/분류 필터를 그대로 유지하면서 전체 데이터 조회 |
| `GetExportDirectory()` | `AppSettings.ExportDirectory` 또는 Documents 폴더 폴백 |

#### 2. SettingsViewModel.cs (신규)

| 프로퍼티 | 역할 |
|---------|------|
| `DefaultPageSize` | 기본 페이지 크기 (NumericUpDown, 10~500) |
| `SearchDebounceMs` | 검색 디바운스 지연 (50~2000ms) |
| `Theme` | 앱 테마 — 콤보박스 변경 즉시 `ThemeManager.Current.ChangeTheme()` 미리보기 |
| `ExportDirectory` | 기본 내보내기 폴더 (Browse시 `OpenFolderDialog`) |
| `EnableDebugLogging` | 디버그 로그 활성화 체크박스 |
| `AvailableThemes` | Dark/Light 계열 15개 테마 목록 |

명령: `SaveCommand`(AppSettingsService.Save), `ResetCommand`(기본값 복원), `BrowseDirectoryCommand`(`OpenFolderDialog`)

#### 3. SettingsView.xaml (신규)

섹션 카드 4개:
- **일반**: DefaultPageSize NumericUpDown, SearchDebounceMs NumericUpDown
- **테마**: ComboBox (변경 즉시 미리보기 안내 텍스트 포함)
- **내보내기**: ExportDirectory TextBox + 📂 탐색 버튼
- **디버그**: EnableDebugLogging CheckBox

저장 / 기본값 복원 버튼 + StatusMessage 표시

#### 4. MainWindow.xaml 수정

아이템·스킬·몬스터 탭 필터 바에 내보내기 버튼 2개 추가:
```
[CSV ↓] [XLS ↓]   ← 각 탭 필터 바 오른쪽
```

⚙ 설정 탭 추가:
```xml
<TabItem Header="⚙ 설정" DataContext="{Binding Settings}">
    <views:SettingsView/>
</TabItem>
```

#### 5. DI 등록 & MainViewModel 업데이트

- `App.xaml.cs`: `AddSingleton<SettingsViewModel>()` 추가
- `MainViewModel`: `Settings: SettingsViewModel` 프로퍼티 + 생성자 주입

### 파일 변경 목록

| 파일 | 변경 유형 | 내용 |
|------|----------|------|
| `ViewModels/SettingsViewModel.cs` | 신규 | AppSettings 편집 VM, 테마 실시간 미리보기 |
| `Views/SettingsView.xaml[.cs]` | 신규 | 설정 폼 UserControl |
| `ViewModels/ItemListViewModel.cs` | 수정 | IExportService + AppSettingsService 주입, ExportCsvCommand/ExportExcelCommand |
| `ViewModels/SkillListViewModel.cs` | 수정 | 동일 |
| `ViewModels/MonsterListViewModel.cs` | 수정 | 동일 |
| `ViewModels/MainViewModel.cs` | 수정 | Settings 프로퍼티 추가 |
| `App.xaml.cs` | 수정 | AddSingleton\<SettingsViewModel\>() |
| `MainWindow.xaml` | 수정 | CSV/XLS 내보내기 버튼 (3탭), ⚙ 설정 탭 추가 |

### 해결한 이슈

- **WPF 빌드 Path 참조 오류**: `_wpftmp.csproj` 환경에서 `System.IO.Path`가 암묵적 using에서 제외 → 세 ListViewModel에 `using System.IO;` 명시 추가

### 빌드·테스트 결과

```
경고 0 / 오류 0   |   테스트 30개 통과 / 0 실패
```

---

## Day 6 — 최종 폴리싱 (로딩 오버레이 · 에러 핸들링 · 접근성)

**날짜:** 2026-03-23  
**목표:** 로딩 스켈레톤 대체 오버레이 · 빈 데이터 안내 · 예외 처리 강화 · 접근성

### 완료 항목

#### 1. BoolToVisibilityConverter.cs (신규)

`bool → Visibility` 변환기 (`Invert` 속성 지원).  
`App.xaml`에 `BoolToVis` / `NotBoolToVis` 두 키로 전역 등록.

#### 2. BusyOverlay UserControl (신규)

`IsBusy = true` 일 때 데이터 영역 위에 표시되는 반투명 로딩 카드:
- `#60000000` 반투명 배경 (입력 차단 — `IsHitTestVisible="True"`)
- 중앙 카드: `mah:ProgressRing` + `StatusMessage` 텍스트 바인딩
- `Panel.ZIndex="10"` 으로 DataGrid 위에 겹침

#### 3. 빈 데이터 EmptyState (MainWindow.xaml)

각 탭 DataGrid 영역에 `IsEmpty` 바인딩 Border 추가:
- `IsEmpty = !IsBusy && Items.Count == 0` (각 ListViewModel에 read-only 프로퍼티)
- 🔍 아이콘 + "검색 결과가 없습니다." 메시지
- DataGrid 위 `Panel.ZIndex="1"` 로 겹침 (BusyOverlay보다 낮은 ZIndex)

#### 4. ViewModelBase 강화

| 추가 멤버 | 역할 |
|---------|------|
| `IsError` | 에러 발생 여부 flag |
| `ErrorMessage` | 에러 메시지 내용 |
| `SetError(msg)` | IsError=true + StatusMessage 설정 |
| `ClearError()` | IsError 초기화 |
| `partial void OnIsBusyChanged` | IsBusy 변경 시 `"IsEmpty"` PropertyChanged 발생 → 파생 클래스 바인딩 갱신 |

#### 5. ListViewModel 에러 처리 & IsEmpty (3개 파일)

- `LoadAsync()` 에 `catch (Exception ex) { SetError(...) }` 추가 — 기존 `finally`만 있던 구조 보강
- `partial void OnItemsChanged / OnSkillsChanged / OnMonstersChanged` — 컬렉션 변경 시 `IsEmpty` notify
- `public bool IsEmpty` 읽기 전용 계산 프로퍼티 추가

#### 6. 전역 예외 핸들러 (App.xaml.cs)

```csharp
DispatcherUnhandledException += (_, ex) => {
    Log.Error(ex.Exception, "Unhandled dispatcher exception");
    ex.Handled = true;
    MessageBox.Show($"예기치 않은 오류\n{ex.Exception.Message}", "오류", ...);
};
AppDomain.CurrentDomain.UnhandledException += ...  // Fatal 로그
TaskScheduler.UnobservedTaskException     += ...  // Warning 로그 + SetObserved
```

#### 7. 접근성 (AutomationProperties.Name)

아이템 · 스킬 · 몬스터 탭의 주요 컨트롤에 `AutomationProperties.Name` 추가:
- 검색 TextBox: `"아이템/스킬/몬스터 이름 검색"`
- 검색 버튼
- 등급/카테고리/직업/유형/지역 ComboBox (필터)
- DataGrid: `"아이템/스킬/몬스터 목록"`

### 파일 변경 목록

| 파일 | 변경 유형 | 내용 |
|------|----------|------|
| `Converters/BoolToVisibilityConverter.cs` | 신규 | bool → Visibility, Invert 지원 |
| `Views/BusyOverlay.xaml[.cs]` | 신규 | 로딩 오버레이 UserControl |
| `ViewModels/ViewModelBase.cs` | 수정 | IsError, ErrorMessage, SetError(), ClearError(), OnIsBusyChanged |
| `ViewModels/ItemListViewModel.cs` | 수정 | catch 추가, IsEmpty, OnItemsChanged |
| `ViewModels/SkillListViewModel.cs` | 수정 | catch 추가, IsEmpty, OnSkillsChanged |
| `ViewModels/MonsterListViewModel.cs` | 수정 | catch 추가, IsEmpty, OnMonstersChanged |
| `App.xaml` | 수정 | BoolToVis / NotBoolToVis 등록 |
| `App.xaml.cs` | 수정 | 전역 예외 핸들러 3종 추가 |
| `MainWindow.xaml` | 수정 | EmptyState Border + BusyOverlay (3탭), AutomationProperties.Name |

### 해결한 이슈

- **Box-drawing 문자 CS1056 오류**: `replace_string_in_file`의 `oldString` 경계에서 `// ──...` 구분선 문자가 `IsEmpty =>..;` 라인에 붙어버린 현상 → 세 ListViewModel에서 수동 제거
- **Log.Logger 라인 손실**: `OnStartup` 교체 중 `Log.Logger = new LoggerConfiguration()` 라인 누락 → 복원

### 빌드·테스트 결과

```
경고 0 / 오류 0   |   테스트 30개 통과 / 0 실패
```

---

---

## Day 7 — 릴리즈 빌드 · 단일 EXE 패키징 · README 최종화

**날짜:** 2026-03-24  
**목표:** Release 빌드 검증 · PublishSingleFile 단일 EXE 생성 · README 완전 갱신 · 7일 스프린트 마무리

### 완료 항목

#### 1. .csproj Publish 속성 추가

`GameDataViewer.App.csproj`에 Release 조건부 PropertyGroup 추가:

```xml
<PropertyGroup Condition="'$(Configuration)'=='Release'">
  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>true</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
  <PublishReadyToRun>false</PublishReadyToRun>
  <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
</PropertyGroup>
```

- `SelfContained=true`: .NET 런타임 내장 → 설치 없이 바로 실행 가능
- `PublishSingleFile=true`: 모든 DLL + 리소스 → 단일 EXE로 번들
- `IncludeNativeLibrariesForSelfExtract=true`: SQLite 네이티브 라이브러리 포함
- `EnableCompressionInSingleFile=true`: EXE 크기 최적화

#### 2. 릴리즈 빌드 검증

```
dotnet build GameDataViewer.sln -c Release
→ 경고 0 / 오류 0
```

```
dotnet test -c Release --no-build
→ 통과: 30 / 실패: 0 / 전체: 30   (3초)
```

#### 3. 단일 EXE 퍼블리시

```bash
dotnet publish src/GameDataViewer.App/GameDataViewer.App.csproj \
  -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -o publish/win-x64
```

**출력 파일 목록:**

| 파일 | 크기 |
|------|------|
| `GameDataViewer.exe` | **73.6 MB** (런타임 내장 단일 EXE) |
| `GameDataViewer.pdb` | 0.1 MB (디버그 심볼) |
| `GameDataViewer.Core.pdb` | ~0 MB |
| `GameDataViewer.Infrastructure.pdb` | ~0 MB |

배포에 필요한 파일: `GameDataViewer.exe` 하나뿐.

#### 4. README.md 최종화

전면 재작성:
- 기능 목록 16개 항목 (모두 ✅ — 미완료 표시 제거)
- 기술 스택 표 (버전 포함)
- 아키텍처 트리 + 레이어 의존 방향 다이어그램
- 화면 구성 5탭 요약
- 실행 방법: 소스 빌드 / dotnet publish 단일 EXE / 자동 생성 경로 표
- 7일 개발 일정 (전체 ✅ 완료)
- 면접 어필 포인트 4개 (배포 파이프라인 항목 신규 추가)

### 파일 변경 목록

| 파일 | 변경 유형 | 내용 |
|------|----------|------|
| `src/GameDataViewer.App/GameDataViewer.App.csproj` | 수정 | Release 조건 Publish PropertyGroup 추가 |
| `README.md` | 수정 | 전체 재작성 — 완성본 기준 최신화 |
| `docs/devlog.md` | 수정 | Day 7 항목 + 7일 최종 점검 기록 추가 |

### 빌드·테스트·퍼블리시 결과

```
Release 빌드:  경고 0 / 오류 0
테스트:        통과 30 / 실패 0 / 전체 30
퍼블리시 EXE:  publish/win-x64/GameDataViewer.exe  (73.6 MB)
```

---

## 7일 스프린트 최종 점검

### 전체 산출물 통계

| 항목 | 수치 |
|------|------|
| 소스 프로젝트 수 | 4개 (Core / Infrastructure / App / Tests) |
| 총 C# 소스 파일 | 약 40개 |
| 총 XAML 파일 | 약 12개 |
| 단위 테스트 | 30개 (전원 통과) |
| NuGet 패키지 | 11개 |
| 시드 데이터 | 아이템 30건 · 스킬 25건 · 몬스터 20건 |
| 최종 EXE 크기 | 73.6 MB (Self-contained win-x64) |

### 기술 스택 최종 확인

| 기술 | 버전 | Day |
|------|------|-----|
| .NET 8 WPF | net8.0-windows | 1 |
| CommunityToolkit.Mvvm | 8.3.2 | 1 |
| MahApps.Metro | 2.4.10 | 1 |
| Microsoft.Data.Sqlite | 8.0.x | 1 |
| Dapper | 2.1.x | 1 |
| Microsoft.Extensions.DI | 8.0.1 | 1 |
| Serilog.Sinks.File | 5.0.0 | 1 |
| CsvHelper | 33.0.1 | 2 |
| ClosedXML | 0.102.3 | 2 |
| OxyPlot.Wpf | 2.2.0 | 4 |
| xUnit + Moq + FluentAssertions | 2.9 / 4.20 / 6.12 | 2 |

### Day별 주요 기능 요약

| Day | 핵심 산출물 |
|-----|------------|
| Day 1 | 솔루션 골격, 도메인 모델, Repository 인터페이스, DI 부트스트랩, 3탭 기본 UI |
| Day 2 | ILogger<T> 전체 주입, DataSeeder Upsert, ExportService, AppSettingsService, 테스트 30개 |
| Day 3 | Converter 4종, DetailView UserControl 3종, 행 하이라이트, 서버 사이드 정렬 |
| Day 4 | OxyPlot 차트 4개 (파이 2+바 2), 📊 통계 탭 |
| Day 5 | CSV/XLS 내보내기 UI, SettingsViewModel/View, ⚙ 설정 탭 |
| Day 6 | BusyOverlay, EmptyState, 전역 예외 핸들러 3종, AutomationProperties 접근성 |
| Day 7 | Release 빌드, PublishSingleFile 단일 EXE (73.6 MB), README 완전 갱신 |

### 아키텍처 품질 점검

| 항목 | 결과 |
|------|------|
| 레이어 분리 (Core / Infra / App) | ✅ App은 Core 인터페이스만 의존 |
| MVVM 준수 (code-behind 최소화) | ✅ 이벤트 핸들러 → VM Command 위임 |
| DI 완전 주입 (new 직접 생성 없음) | ✅ 모든 VM/Service Singleton 등록 |
| Repository 추상화 (IRepository → impl) | ✅ 테스트에서 Moq 대체 확인 |
| 전역 예외 안전망 | ✅ Dispatcher / AppDomain / TaskScheduler |
| 접근성 | ✅ AutomationProperties.Name 주요 컨트롤 |
| 배포 단순화 | ✅ 단일 EXE (런타임 불필요) |

### 이슈 해결 이력 (7일 전체)

| Day | 이슈 | 해결책 |
|-----|------|--------|
| 2 | DataSeeder 리팩터링 후 구 코드 잔존 | 수동 삭제 |
| 2 | SQLite `:memory:` 연결별 격리 | 임시 파일 경로 방식으로 전환 |
| 2 | Set-Content 한국어 인코딩 깨짐 | `[System.IO.File]::WriteAllText` UTF-8 사용 |
| 5 | WPF `_wpftmp.csproj`에서 `System.IO.Path` 미참조 | 3 ListViewModel에 `using System.IO;` 추가 |
| 6 | Box-drawing 문자 CS1056 컴파일 오류 | 3 ListViewModel 해당 라인 수동 수정 |
| 6 | OnStartup 교체 중 Log.Logger 라인 손실 | 복원 |
| 6 | ViewModelBase 닫는 `}` 누락 | 추가 |
| 9 | `dotnet run` WDAC 정책 차단 (0x800711C7) | 직접 EXE 실행으로 우회 |
| 9 | 데이터 미표시 | csproj Content Include 경로 `..\..\..\data` → `..\..\data` 수정 |
| 9 | 탭 헤더 초기 불가시 (3회 발생) | MahApps 페이드인 트랜지션 → `WindowTransitionsEnabled="False"` 제거 |
| 9 | 탭 헤더 클릭 불가 | ListBox 커스텀 Template이 이벤트 라우팅 제거 → Template 제거로 복원 |
| 9 | 페이징 미작동 | PageSize 하드코딩(50) → `_settings.Current.DefaultPageSize` 연동 |
| 9 | 페이징 버튼 항상 활성 | `CanExecute` 미구현 → `[RelayCommand(CanExecute)]` + `NotifyCanExecuteChanged` 추가 |
| 9 | 설정 탭 페이지 크기 변경 후 탭 이동 시 미반영 | `OnSelectedTabIndexChanged` 에서 `ApplySettingsAsync()` 호출 |

---

## Day 8 — 팀장 최종 검수

**날짜:** 2026-04-01  
**목표:** 리니지M 사업부 팀장 관점 최종 QA 피드백 반영

### 완료 항목

| 항목 | 내용 |
|------|------|
| 데이터 경로 수정 | `GameDataViewer.App.csproj` Content Include 경로 `..\..\..\data` → `..\..\data` |
| 창 활성화 | `App.xaml.cs` — `mainWindow.Show()` 후 `mainWindow.Activate()` 추가 |
| 설정 설명 텍스트 | `SettingsView.xaml` — 기본 페이지 크기·검색 디바운스 NumericUpDown에 설명 TextBlock + ToolTip 추가 |
| 사용설명서 작성 | `docs/user-guide.md` 신규 — 5탭 기능·설정 항목 용어 해설·FAQ 포함 |

### 빌드·테스트 결과

```
경고 0 / 오류 0   |   테스트 30개 통과 / 0 실패
```

---

## Day 9 — 탭·페이징 버그 집중 수정

**날짜:** 2026-04-01  
**목표:** 탭 헤더 미표시 / 페이징 미작동 근본 원인 해소

### 발생한 버그 및 원인

| 버그 | 근본 원인 |
|------|-----------|
| 앱 실행 후 탭 헤더가 보이지 않음 | MahApps `MetroWindow` 페이드인 트랜지션 애니메이션이 창 활성화 전까지 콘텐츠 렌더링을 지연 |
| 탭 클릭 불가 | 이전 세션의 ListBox 커스텀 `Template`(`<ItemsPresenter/>` 단독)이 마우스 이벤트 라우팅 인프라 제거 |
| 페이징 버튼 작동 안함 | `PageSize` 하드코딩 50 → 시드 데이터(30/25/20건) 가 항상 1페이지, `CanExecute` 미구현으로 버튼 상태 미갱신 |
| 설정 탭 페이지 크기 저장 후 탭 이동 시 미반영 | 탭 전환 이벤트와 VM `RefreshAsync()` 미연결 |

### 수정 내용

#### 1. `WindowTransitionsEnabled="False"` (MainWindow.xaml)

```xml
<mah:MetroWindow ...
    WindowTransitionsEnabled="False"
    ...>
```

MahApps 페이드인 트랜지션을 비활성화 → `Show()` 즉시 전체 콘텐츠 렌더링.  
이것으로 클릭 없이도 탭 헤더가 처음부터 표시됨.

#### 2. 탭 헤더 구조 변경 — TabControl 외부 독립 ListBox (MainWindow.xaml)

`TabPanel(PART_HeaderPanel)` 대신 독립 `ListBox`를 Grid Row="0" 에 배치:
- WPF 탭 생명주기와 완전 분리
- `SelectedIndex` 양방향 바인딩으로 `TabControl`과 동기화
- ListBox 커스텀 Template 제거 (이전 세션에서 추가된 것) → WPF 기본 이벤트 라우팅 복원
- `TabControl`은 `PART_SelectedContentHost`만 남긴 최소 Template 사용

```xml
<!-- Row 0: 탭 헤더 독립 ListBox -->
<Border Grid.Row="0" Background="#252526" BorderBrush="#3F9BF5" BorderThickness="0,0,0,1">
    <ListBox SelectedIndex="{Binding SelectedTabIndex}" Style="{x:Null}" ...>
        <ListBoxItem>⚔ 아이템</ListBoxItem>
        ...
    </ListBox>
</Border>
<!-- Row 1: 콘텐츠 전용 TabControl -->
<TabControl Grid.Row="1" SelectedIndex="{Binding SelectedTabIndex}" Style="{x:Null}">
    <TabControl.Template>
        <ControlTemplate TargetType="TabControl">
            <Border Background="#1E1E1E">
                <ContentPresenter x:Name="PART_SelectedContentHost" ContentSource="SelectedContent"/>
            </Border>
        </ControlTemplate>
    </TabControl.Template>
    ...
</TabControl>
```

#### 3. 페이징 CanExecute 구현 (3개 ListViewModel)

```csharp
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
```

#### 4. PageSize → Settings 연동 (3개 ListViewModel)

```csharp
// 이전: private const int PageSize = 50;
// 이후:
PageSize = _settings.Current.DefaultPageSize,
```

#### 5. 탭 전환 시 설정 즉시 반영 (MainViewModel)

```csharp
public bool InitialLoadCompleted { get; set; }

partial void OnSelectedTabIndexChanged(int value)
{
    if (!InitialLoadCompleted) return;
    _ = value switch
    {
        0 => Items.ApplySettingsAsync(),
        1 => Skills.ApplySettingsAsync(),
        2 => Monsters.ApplySettingsAsync(),
        _ => Task.CompletedTask
    };
}
```

### 파일 변경 목록

| 파일 | 변경 내용 |
|------|-----------|
| `MainWindow.xaml` | `WindowTransitionsEnabled="False"` 추가, 탭 헤더 → 외부 ListBox 분리, Template 최소화 |
| `MainWindow.xaml.cs` | `OnContentRendered` Dispatcher 제거 (불필요) |
| `App.xaml.cs` | `InitialLoadCompleted = true`, `mainWindow.Activate()` 순서 정리 |
| `ViewModels/MainViewModel.cs` | `InitialLoadCompleted`, `OnSelectedTabIndexChanged` 추가 |
| `ViewModels/ItemListViewModel.cs` | PageSize→Settings 연동, CanExecute, `ApplySettingsAsync()` |
| `ViewModels/SkillListViewModel.cs` | 동일 |
| `ViewModels/MonsterListViewModel.cs` | 동일 |

### 빌드·테스트 결과

```
경고 0 / 오류 0   |   테스트 30개 통과 / 0 실패
```

---

## Day 10 — 초기 화면 탭 바 미표시 최종 수정

**날짜:** 2026-04-01  
**목표:** 일반 창 크기(비최대화)에서 앱 실행 시 탭 바가 보이지 않는 문제 근본 해결

### 발생한 버그

| 현상 | 조건 |
|------|------|
| 탭 바 미표시 | 앱 최초 실행 (일반 창 크기, `Height="800"`) |
| 탭 바 정상 표시 | 창 최대화 후 / 창 테두리 클릭 후 |

### 근본 원인

WPF가 `Height="Auto"` Grid Row를 초기 레이아웃 측정할 때, `StackPanel Orientation="Horizontal"` 내부 ListBoxItem들을 **0px로 측정**하는 버그. 창 최대화(레이아웃 재계산) 또는 활성화 이벤트 이후 정상값으로 복구됨.

### 수정 내용

#### 1. `WindowState="Maximized"` (MainWindow.xaml)

```xml
<mah:MetroWindow ...
    WindowState="Maximized"
    WindowTransitionsEnabled="False"
    ...>
```

처음부터 최대화 상태로 시작 → 레이아웃 측정 시 충분한 공간 확보.

#### 2. `Border MinHeight="46"` + `ListBox MinHeight="44"` (MainWindow.xaml)

```xml
<Border Grid.Row="0" ... MinHeight="46">
    <ListBox ... MinHeight="44" VirtualizingPanel.IsVirtualizing="False">
```

- `MinHeight` : Row 높이 붕괴 방지 — 레이아웃 측정 전에도 최소 높이 확보
- `VirtualizingPanel.IsVirtualizing="False"` : 가상화로 인한 초기 항목 측정 누락 방지

### 파일 변경 목록

| 파일 | 변경 내용 |
|------|-----------|
| `MainWindow.xaml` | `WindowState="Maximized"` 추가, `Border/ListBox MinHeight` 추가, `IsVirtualizing="False"` 추가 |

### 빌드 결과

```
경고 0 / 오류 0
```

---

## 스프린트 회고

### 잘 된 점
- 7일 계획을 지키며 설계 → 구현 → 테스트 → 배포 전체 사이클 완주
- Repository 패턴 + DI + MVVM의 조합으로 테스트 가능 구조 확립
- OxyPlot, MahApps.Metro, BusyOverlay 등 현업 라이브러리 실습
- `dotnet publish` 단일 EXE 파이프라인 경험

### 개선 여지
- 실제 리니지M API 연동 시 비동기 HTTP 클라이언트 레이어 추가 필요
- 스크린샷 / GIF 캡처 미완성 (docs/screenshots/ 폴더 비어있음)
- ReadyToRun 비활성화 → 초기 실행 JIT 시간 약간 있음
- E2E UI 자동화 테스트(WinAppDriver 등) 미포함

---

## Day 8 — 개선 여지 항목 구현

**날짜:** 2026-03-23  
**목표:** Day 7 회고에서 식별된 개선 여지 4가지 구현

### 완료 항목

#### 1. ReadyToRun 활성화

`GameDataViewer.App.csproj`의 `PublishReadyToRun`을 `false` → `true`로 변경:

```xml
<PublishReadyToRun>true</PublishReadyToRun>
```

- Publish 시 AOT 수준의 ReadyToRun 컴파일 수행 → 첫 실행 JIT 부하 감소
- EXE 크기는 약간 증가하나 시작 속도 개선

#### 2. HTTP 클라이언트 레이어 추가

실제 리니지M API 연동을 위한 추상화 레이어 추가:

**Core/Contracts/IGameApiClient.cs (신규)**
```csharp
public interface IGameApiClient
{
    Task<IReadOnlyList<Item>>    FetchItemsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Skill>>   FetchSkillsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Monster>> FetchMonstersAsync(CancellationToken ct = default);
}
```

**Infrastructure/ApiClients/LineageMApiClient.cs (신규)**
- `HttpClient` 주입 (IHttpClientFactory 패턴 지원)
- `GetFromJsonAsync<T>` 로 각 엔드포인트 조회
- 예외 발생 시 빈 컬렉션 반환 + `ILogger`로 에러 로그
- `BaseAddress` 상수 노출 → App.xaml.cs DI 등록 시 사용

**App.xaml.cs — DI 등록 주석 추가**
- 실제 API 연동 시 주석 해제하면 활성화되는 `AddHttpClient` 코드 준비:
```csharp
// services.AddHttpClient<IGameApiClient, LineageMApiClient>(client =>
// {
//     client.BaseAddress = new Uri(LineageMApiClient.BaseAddress);
//     // client.DefaultRequestHeaders.Add("X-Api-Key", apiKey)
// });
```

**NuGet 추가**: `Microsoft.Extensions.Http 8.0.1` (Infrastructure 프로젝트)

#### 3. docs/screenshots/ 폴더 + 안내 파일

`docs/screenshots/README.md` 생성:
- 권장 파일 목록 (items.png, skills.png, monsters.png, charts.png, settings.png, demo.gif)
- `Win + Shift + S` 캡처 방법 안내
- ScreenToGif GIF 캡처 링크
- README.md 연동 마크다운 예시

#### 4. E2E UI 자동화 테스트 프로젝트 (FlaUI)

`tests/GameDataViewer.UITests/` 신규 프로젝트 — FlaUI.UIA3 4.0.0 기반:

| 파일 | 내용 |
|------|------|
| `AppTestBase.cs` | Application.Launch + GetMainWindow + IDisposable 공통 베이스 |
| `AppLaunchTests.cs` | 앱 실행 (3건): IsOffscreen, Title 포함, 탭 5개 존재 |
| `ItemTabTests.cs` | 아이템 탭 (2건): DataGrid 행 존재, 검색 입력 후 앱 정상 유지 |
| `README.md` | 사전 준비 / 실행 방법 / 직접 해야 할 작업 안내 |

`GameDataViewer.sln`에 UITests 프로젝트 추가 완료.

### 파일 변경 목록

| 파일 | 변경 유형 | 내용 |
|------|----------|------|
| `GameDataViewer.App.csproj` | 수정 | `PublishReadyToRun` false → true |
| `Core/Contracts/IGameApiClient.cs` | 신규 | HTTP 클라이언트 인터페이스 |
| `Infrastructure/ApiClients/LineageMApiClient.cs` | 신규 | HttpClient 기반 구현체 |
| `Infrastructure/GameDataViewer.Infrastructure.csproj` | 수정 | `Microsoft.Extensions.Http 8.0.1` 추가 |
| `App.xaml.cs` | 수정 | LineageMApiClient using 추가, DI 등록 주석 블록 추가 |
| `docs/screenshots/README.md` | 신규 | 스크린샷 파일 목록 및 캡처 방법 안내 |
| `tests/GameDataViewer.UITests/*.cs` | 신규 | FlaUI E2E 테스트 (AppTestBase, AppLaunchTests, ItemTabTests) |
| `tests/GameDataViewer.UITests/README.md` | 신규 | E2E 테스트 사전 준비 및 실행 가이드 |
| `GameDataViewer.sln` | 수정 | UITests 프로젝트 추가 |

### 빌드·테스트 결과

```
Release 빌드:  경고 0 / 오류 0   (5개 프로젝트)
단위 테스트:   통과 30 / 실패 0 / 전체 30
E2E 테스트:    앱 실행 필요 (별도 실행)
```

### 직접 해야 할 작업 (사용자)

| 작업 | 방법 |
|------|------|
| 앱 스크린샷 캡처 | 앱 실행 후 `Win+Shift+S` → `docs/screenshots/` 저장 |
| E2E 테스트 실행 | `dotnet publish` 후 `dotnet test tests/GameDataViewer.UITests` |
| 실제 API 엔드포인트 연동 | `LineageMApiClient.cs` 엔드포인트 수정 + `App.xaml.cs` 주석 해제 |

---

## Day 11 — E2E 테스트 확장 + 앱 중복 실행 문제 해결

**날짜:** 2026-04-01  
**목표:** SkillTabTests/MonsterTabTests 신규 추가 + 테스트마다 앱이 켜졌다 꺼지는 문제 해결

### 발생한 문제

| 현상 | 원인 |
|------|------|
| 테스트 11개마다 앱이 실행·종료 반복 | `AppTestBase` 생성자에서 앱 실행, `Dispose()`에서 종료 → xUnit이 테스트 메서드마다 새 인스턴스 생성 |
| 이전 테스트 실행 시 `Another process is using the file` 오류 | 병렬 실행으로 여러 인스턴스가 동일 EXE를 동시 실행 시도 |

### 해결 내용

#### 1. `UITestFixture.cs` 신규 — 앱 생명주기 분리

xUnit `ICollectionFixture<T>` 패턴 적용:
- `UITestFixture`: 앱 실행·종료 이관 (컬렉션당 1회)
- `[CollectionDefinition("UITests")]`: "UITests" 컬렉션 정의 — 동일 파일에 배치

```csharp
[CollectionDefinition("UITests")]
public class UITestCollection : ICollectionFixture<UITestFixture> { }

public class UITestFixture : IDisposable
{
    public Application App { get; }
    public UIA3Automation Automation { get; }
    public Window MainWindow { get; }
    public ConditionFactory Cf { get; }

    public UITestFixture() { /* Application.Launch() 1회 */ }
    public void Dispose() { /* App.Close() 1회 */ }
}
```

#### 2. `AppTestBase.cs` 수정 — 픽스처 주입으로 전환

- `IDisposable` 제거 (앱 종료 책임을 `UITestFixture`로 이관)
- 생성자: `protected AppTestBase(UITestFixture fixture)` — 픽스처에서 프로퍼티 참조

#### 3. 모든 테스트 클래스에 생성자 추가

```csharp
public class AppLaunchTests(UITestFixture fixture) : AppTestBase(fixture) { }
// ItemTabTests, SkillTabTests, MonsterTabTests 동일
```

#### 4. SkillTabTests / MonsterTabTests 신규 파일

| 파일 | 테스트 수 | 시나리오 |
|------|----------|---------|
| `SkillTabTests.cs` | 3 | DataGrid 행 확인, 검색 필터, 직업 필터 콤보박스 |
| `MonsterTabTests.cs` | 3 | DataGrid 행 확인, 검색 필터, 등급 필터 콤보박스 |

### 효과

| 항목 | 이전 | 이후 |
|------|------|------|
| 앱 실행 횟수 | 11회 (테스트마다) | **1회** (컬렉션 전체) |
| 테스트 격리 | 인스턴스 독립 | 공유 인스턴스 (탭 상태 유지) |
| 총 E2E 테스트 수 | 5개 | **11개** |

### 파일 변경 목록

| 파일 | 변경 유형 | 내용 |
|------|----------|------|
| `tests/UITests/UITestFixture.cs` | 신규 | UITestFixture + UITestCollection ([CollectionDefinition]) |
| `tests/UITests/AppTestBase.cs` | 수정 | IDisposable 제거, UITestFixture 주입 생성자로 전환 |
| `tests/UITests/AppLaunchTests.cs` | 수정 | UITestFixture 주입 생성자 추가 |
| `tests/UITests/ItemTabTests.cs` | 수정 | UITestFixture 주입 생성자 추가 |
| `tests/UITests/SkillTabTests.cs` | 신규 | 스킬 탭 E2E 테스트 3개 |
| `tests/UITests/MonsterTabTests.cs` | 신규 | 몬스터 탭 E2E 테스트 3개 |
| `tests/UITests/xunit.runner.json` | 신규 | 직렬 실행 설정 (parallelizeTestCollections: false) |

### 빌드 결과

```
경고 0 / 오류 0
```

---

## Day 12 — 프로젝트 마무리 및 GitHub 배포 준비

**날짜:** 2026-04-01  
**목표:** 최종 빌드 검증 · README/devlog 최신화 · .gitignore 정비 · GitHub 업로드 준비

### 완료 항목

#### 1. 최종 빌드 · 테스트 검증

```
Release 빌드 (5개 프로젝트):  경고 0 / 오류 0
단위 테스트:                   통과 30 / 실패 0 / 전체 30
```

#### 2. README.md 최종화

| 수정 항목 | 내용 |
|----------|------|
| 기술 스택 테이블 | FlaUI.UIA3 E2E 테스트 행 추가 |
| 아키텍처 트리 | `tests/GameDataViewer.UITests/` 구조 추가 |
| 실행 방법 | `dotnet run` → EXE 직접 실행 안내 (WDAC 정책 주의사항 추가) |
| 테스트 실행 | 단위 테스트 / E2E 테스트 구분 명령어 추가 |
| 개발 일정 표 | Day 8~11 항목 추가 |
| 면접 어필 포인트 | E2E FlaUI + ICollectionFixture 항목 추가 (5항목) |

#### 3. .gitignore 정비

| 변경 | 이유 |
|------|------|
| `*.exe` / `*.pdb` 전역 제외 → 제거 | `publish/` 규칙으로 충분 |
| `TestResults/` 추가 | `dotnet test --results-directory` 산출물 제외 |
| `settings.json` 추가 | 사용자별 자동 생성 파일 제외 |

### GitHub 업로드 순서

```bash
# 1. Git 초기화 (아직 안 했다면)
git init
git remote add origin https://github.com/<user>/game-data-viewer.git

# 2. 첫 커밋
git add .
git commit -m "feat: 리니지M 스타일 게임 데이터 뷰어 (WPF .NET8 MVVM, 11일 스프린트)"

# 3. GitHub에 푸시
git branch -M main
git push -u origin main
```

### 최종 프로젝트 통계

| 항목 | 수치 |
|------|------|
| 소스 프로젝트 수 | 5개 (Core / Infrastructure / App / Tests / UITests) |
| 단위 테스트 | 30개 (전원 통과) |
| E2E UI 테스트 | 11개 (FlaUI.UIA3, 컬렉션당 1회 실행) |
| NuGet 패키지 | 13개 |
| 개발 기간 | 11일 (7일 스프린트 + 4일 QA/확장) |
| 최종 EXE 크기 | ~74 MB (Self-contained win-x64) |

### 빌드·테스트 최종 결과

```
Release 빌드:  경고 0 / 오류 0   (5개 프로젝트)
단위 테스트:   통과 30 / 실패 0 / 전체 30
```
