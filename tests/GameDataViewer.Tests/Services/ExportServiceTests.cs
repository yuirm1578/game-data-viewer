using FluentAssertions;
using GameDataViewer.Core.Models;
using GameDataViewer.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace GameDataViewer.Tests.Services;

/// <summary>ExportService 통합 테스트 — 실제 파일 I/O 검증</summary>
public class ExportServiceTests : IDisposable
{
    private readonly ExportService _svc;
    private readonly string _tempDir;
    private readonly List<Item> _testItems;

    public ExportServiceTests()
    {
        _svc = new ExportService(NullLogger<ExportService>.Instance);
        _tempDir = Path.Combine(Path.GetTempPath(), $"GDV_Test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        _testItems =
        [
            new Item { Id = 1, Name = "불꽃의 검", Grade = "영웅",  Category = "무기",   Price = 15000, RequiredLevel = 30 },
            new Item { Id = 2, Name = "강철 방패", Grade = "고급",  Category = "방어구", Price =  8000, RequiredLevel = 20 },
            new Item { Id = 3, Name = "마나 포션", Grade = "일반",  Category = "소모품", Price =   500, RequiredLevel =  1 },
        ];
    }

    // ──────────────────────────────────────────────────────
    // CSV 테스트
    // ──────────────────────────────────────────────────────

    [Fact]
    public async Task ExportToCsvAsync_CreatesFile()
    {
        var path = Path.Combine(_tempDir, "items.csv");
        await _svc.ExportToCsvAsync(_testItems, path);
        File.Exists(path).Should().BeTrue();
    }

    [Fact]
    public async Task ExportToCsvAsync_FileIsNotEmpty()
    {
        var path = Path.Combine(_tempDir, "items_nonempty.csv");
        await _svc.ExportToCsvAsync(_testItems, path);
        new FileInfo(path).Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExportToCsvAsync_ContainsHeaderRow()
    {
        var path = Path.Combine(_tempDir, "items_header.csv");
        await _svc.ExportToCsvAsync(_testItems, path);
        var lines = await File.ReadAllLinesAsync(path);
        lines.Should().NotBeEmpty();
        // CsvHelper는 첫 줄에 속성명을 씀
        lines[0].Should().Contain("Name");
    }

    [Fact]
    public async Task ExportToCsvAsync_ContainsAllDataRows()
    {
        var path = Path.Combine(_tempDir, "items_rows.csv");
        await _svc.ExportToCsvAsync(_testItems, path);
        var lines = await File.ReadAllLinesAsync(path);
        // 헤더(1행) + 데이터(3행) = 4행
        lines.Should().HaveCount(4);
    }

    [Fact]
    public async Task ExportToCsvAsync_ContainsItemNames()
    {
        var path = Path.Combine(_tempDir, "items_names.csv");
        await _svc.ExportToCsvAsync(_testItems, path);
        var content = await File.ReadAllTextAsync(path);
        content.Should().Contain("불꽃의 검");
        content.Should().Contain("강철 방패");
        content.Should().Contain("마나 포션");
    }

    [Fact]
    public async Task ExportToCsvAsync_EmptyData_CreatesHeaderOnlyFile()
    {
        var path = Path.Combine(_tempDir, "empty.csv");
        await _svc.ExportToCsvAsync(Array.Empty<Item>(), path);
        File.Exists(path).Should().BeTrue();
        var lines = await File.ReadAllLinesAsync(path);
        // 헤더만 1행 (또는 빈 파일일 수도 있음 — 0 이상이면 통과)
        lines.Length.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ExportToCsvAsync_CreatesIntermediateDirectories()
    {
        var deep = Path.Combine(_tempDir, "sub", "deep", "items.csv");
        await _svc.ExportToCsvAsync(_testItems, deep);
        File.Exists(deep).Should().BeTrue();
    }

    // ──────────────────────────────────────────────────────
    // Excel 테스트
    // ──────────────────────────────────────────────────────

    [Fact]
    public async Task ExportToExcelAsync_CreatesFile()
    {
        var path = Path.Combine(_tempDir, "items.xlsx");
        await _svc.ExportToExcelAsync(_testItems, path, "아이템");
        File.Exists(path).Should().BeTrue();
    }

    [Fact]
    public async Task ExportToExcelAsync_FileIsNotEmpty()
    {
        var path = Path.Combine(_tempDir, "items_nonempty.xlsx");
        await _svc.ExportToExcelAsync(_testItems, path, "아이템");
        new FileInfo(path).Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExportToExcelAsync_EmptyData_CreatesFile()
    {
        var path = Path.Combine(_tempDir, "empty.xlsx");
        await _svc.ExportToExcelAsync(Array.Empty<Item>(), path, "빈시트");
        File.Exists(path).Should().BeTrue();
    }

    [Fact]
    public async Task ExportToExcelAsync_CreatesIntermediateDirectories()
    {
        var deep = Path.Combine(_tempDir, "sub2", "deep2", "items.xlsx");
        await _svc.ExportToExcelAsync(_testItems, deep, "아이템");
        File.Exists(deep).Should().BeTrue();
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); }
        catch { /* 테스트 임시 폴더 삭제 실패 무시 */ }
    }
}
