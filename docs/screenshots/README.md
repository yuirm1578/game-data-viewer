# 스크린샷 폴더

이 폴더에 앱 스크린샷 / GIF를 저장합니다.

## 권장 파일 목록

| 파일명 | 내용 |
|--------|------|
| `items.png` | ⚔ 아이템 탭 — DataGrid + 상세 사이드 패널 |
| `skills.png` | ✨ 스킬 탭 — 검색/필터 + 상세 패널 |
| `monsters.png` | 👾 몬스터 탭 — DataGrid + BOSS 뱃지 |
| `charts.png` | 📊 통계 탭 — 파이차트 2개 + 바차트 2개 |
| `settings.png` | ⚙ 설정 탭 — 테마 전환 / 내보내기 폴더 |
| `demo.gif` | 앱 전체 동작 데모 GIF (선택) |

## 캡처 방법

### 정적 PNG 캡처
1. 앱을 실행합니다: `dotnet run --project src/GameDataViewer.App/GameDataViewer.App.csproj`
2. 원하는 탭으로 이동합니다.
3. `Win + Shift + S` → 캡처 후 이 폴더에 해당 파일명으로 저장합니다.

### GIF 캡처 (선택)
- [ScreenToGif](https://www.screentogif.com/) (무료, 권장)
- 녹화 후 `demo.gif`로 저장합니다.

## README 연동

[README.md](../../README.md)의 스크린샷 섹션에 아래처럼 이미지를 추가하세요:

```markdown
| 아이템 탭 | 통계 탭 | 설정 탭 |
|-----------|---------|---------|
| ![items](docs/screenshots/items.png) | ![charts](docs/screenshots/charts.png) | ![settings](docs/screenshots/settings.png) |
```
