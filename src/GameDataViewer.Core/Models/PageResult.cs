namespace GameDataViewer.Core.Models;

/// <summary>페이징 요청</summary>
public class PageRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SearchTerm { get; set; }
    public string? Grade { get; set; }
    public string? Category { get; set; }
    public string? SortColumn { get; set; }
    public bool SortDescending { get; set; }
}

/// <summary>페이징 결과</summary>
public class PageResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
