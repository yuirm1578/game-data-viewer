# GameDataViewer.UITests — E2E UI 자동화 테스트

FlaUI(UIA3) 기반 WPF 앱 End-to-End 자동화 테스트 프로젝트입니다.

---

## 사전 준비

### 1. 앱 빌드 (필수)
E2E 테스트는 빌드된 실행 파일을 직접 실행합니다. 먼저 publish를 진행하세요:

```bash
cd "c:\Game Data Viewer"
dotnet publish src/GameDataViewer.App/GameDataViewer.App.csproj `
  -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true `
  -o publish/win-x64
```

### 2. 앱 경로 설정 (선택)
기본적으로 `publish\win-x64\GameDataViewer.exe`를 자동으로 탐색합니다.  
다른 경로를 사용하려면 환경 변수를 설정하세요:

```powershell
$env:APP_PATH = "C:\path\to\GameDataViewer.exe"
```

### 3. Windows 환경 필요
FlaUI는 Windows UI Automation(UIA3)을 사용하므로 **Windows 전용** 테스트입니다.  
Linux/macOS CI에서는 이 테스트를 건너뛰세요.

---

## 테스트 실행

```bash
# E2E 테스트만 실행
dotnet test tests/GameDataViewer.UITests/GameDataViewer.UITests.csproj -v normal

# 단위 테스트와 함께 전체 실행
dotnet test --filter "FullyQualifiedName~UITests" -v normal
```

> 테스트 실행 시 실제 앱(GameDataViewer.exe)이 화면에 열렸다가 닫힙니다.  
> 화면 잠금 상태나 헤드리스 환경에서는 실패할 수 있습니다.

---

## 테스트 구성

| 파일 | 테스트 수 | 시나리오 |
|------|----------|---------|
| `AppLaunchTests.cs` | 3 | 앱 실행, 윈도우 타이틀, 탭 5개 존재 확인 |
| `ItemTabTests.cs` | 2 | 아이템 DataGrid 행 존재, 검색 텍스트 입력 후 앱 정상 유지 |

---

## CI/CD 통합 (GitHub Actions 예시)

```yaml
- name: UI Tests (E2E)
  if: runner.os == 'Windows'
  run: dotnet test tests/GameDataViewer.UITests --no-build -v normal
  env:
    APP_PATH: ${{ github.workspace }}/publish/win-x64/GameDataViewer.exe
```

---

## 직접 해야 할 작업

- [ ] 솔루션에 UITests 프로젝트 추가: `dotnet sln add tests/GameDataViewer.UITests/GameDataViewer.UITests.csproj`
- [ ] `dotnet restore tests/GameDataViewer.UITests` 로 FlaUI 패키지 복원
- [ ] 스킬 탭 / 몬스터 탭 / 통계 탭 / 설정 탭 E2E 테스트 추가 (현재 아이템 탭만 구현됨)
- [ ] CI 환경(GitHub Actions)에서 Windows runner를 사용하는 경우 workflow 파일 작성
