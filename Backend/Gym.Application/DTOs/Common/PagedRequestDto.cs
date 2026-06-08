namespace Gym.Application.DTOs.Common;

public class PagedRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string SortColumn { get; set; } = "Name";
    public string SortDirection { get; set; } = "asc";
}
