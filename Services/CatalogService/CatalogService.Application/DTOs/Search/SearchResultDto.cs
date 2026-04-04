namespace CatalogService.Application.DTOs.Search;

using CatalogService.Domain.Enums;

public class SearchResultDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public CuisineType CuisineType { get; set; }

    public decimal Rating { get; set; }

    public int? DeliveryTime { get; set; }

    public double? Distance { get; set; }

    public string? ImageUrl { get; set; }
}
