namespace GameDataViewer.Core.Contracts;

/// <summary>데이터 내보내기 서비스</summary>
public interface IExportService
{
    Task ExportToCsvAsync<T>(IEnumerable<T> data, string filePath, CancellationToken ct = default);
    Task ExportToExcelAsync<T>(IEnumerable<T> data, string filePath, string sheetName = "Data", CancellationToken ct = default);
}
