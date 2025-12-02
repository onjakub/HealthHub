namespace HealthHub.Application.DTOs;

public class PaginationResponseDto<T>
{
    public IEnumerable<T> Nodes { get; set; } = new List<T>();
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