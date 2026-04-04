namespace CatalogService.Application.DTOs.Pagination;

public class PaginatedResultDto<T>
{
    public List<T> Data { get; set; } = [];

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

    public bool HasNextPage => PageNumber < TotalPages;

    public bool HasPreviousPage => PageNumber > 1;
}
