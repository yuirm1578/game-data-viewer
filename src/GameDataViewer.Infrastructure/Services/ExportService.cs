using System.Globalization;
using System.Reflection;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using GameDataViewer.Core.Contracts;
using Microsoft.Extensions.Logging;

namespace GameDataViewer.Infrastructure.Services;

/// <summary>CSV / Excel 내보내기 서비스 구현</summary>
public class ExportService : IExportService
{
    private readonly ILogger<ExportService> _logger;

    public ExportService(ILogger<ExportService> logger) => _logger = logger;

    // ────────────────────────────────────────────────────
    // CSV
    // ────────────────────────────────────────────────────
    public async Task ExportToCsvAsync<T>(
        IEnumerable<T> data,
        string filePath,
        CancellationToken ct = default)
    {
        _logger.LogInformation("Exporting CSV → {Path}", filePath);
        EnsureDirectory(filePath);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Encoding = System.Text.Encoding.UTF8
        };

        await using var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
        await using var csv = new CsvWriter(writer, config);
        await csv.WriteRecordsAsync(data, ct);
        _logger.LogInformation("CSV export complete.");
    }

    // ────────────────────────────────────────────────────
    // Excel (ClosedXML)
    // ────────────────────────────────────────────────────
    public async Task ExportToExcelAsync<T>(
        IEnumerable<T> data,
        string filePath,
        string sheetName = "Data",
        CancellationToken ct = default)
    {
        _logger.LogInformation("Exporting Excel → {Path}", filePath);
        EnsureDirectory(filePath);

        await Task.Run(() =>
        {
            var list = data.ToList();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);

            // 헤더 행
            for (int col = 0; col < props.Length; col++)
            {
                var cell = ws.Cell(1, col + 1);
                cell.Value = props[col].Name;
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1, 0.5);
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // 데이터 행
            for (int row = 0; row < list.Count; row++)
            {
                for (int col = 0; col < props.Length; col++)
                {
                    var value = props[col].GetValue(list[row]);
                    var cell = ws.Cell(row + 2, col + 1);

                    cell.Value = value switch
                    {
                        bool b   => b ? "Y" : "N",
                        null     => string.Empty,
                        var v    => XLCellValue.FromObject(v)
                    };
                }
            }

            // 열 너비 자동 조정
            ws.Columns().AdjustToContents();

            // 헤더 행 고정
            ws.SheetView.FreezeRows(1);

            wb.SaveAs(filePath);
        }, ct);

        _logger.LogInformation("Excel export complete. Rows={Count}", data.Count());
    }

    private static void EnsureDirectory(string filePath)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);
    }
}
