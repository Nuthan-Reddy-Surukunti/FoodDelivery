namespace OrderService.Application.DTOs.Requests;

using OrderService.Domain.Enums;

public class CreateUserAddressRequestDto
{
    public string AddressLine1 { get; set; } = string.Empty;

    public string? AddressLine2 { get; set; }

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public AddressType AddressType { get; set; } = AddressType.Home;

    public bool IsDefault { get; set; }
}
