namespace HealthHub.Maui.Models;

public class PaginationResponseDto<T>
{
    public IEnumerable<T> Nodes { get; set; } = Enumerable.Empty<T>();
    public PageInfoDto PageInfo { get; set; } = new();
    public int TotalCount { get; set; }
}

public class PageInfoDto
{
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public string? StartCursor { get; set; }
    public string? EndCursor { get; set; }
}

public class DiagnosisFilter
{
    public string? Type { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
}