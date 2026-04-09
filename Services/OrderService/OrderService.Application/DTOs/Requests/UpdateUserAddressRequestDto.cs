namespace OrderService.Application.DTOs.Requests;

using OrderService.Domain.Enums;

public class UpdateUserAddressRequestDto
{
    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? PostalCode { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public AddressType? AddressType { get; set; }

    public bool? IsDefault { get; set; }
}
