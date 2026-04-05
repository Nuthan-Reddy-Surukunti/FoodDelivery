namespace OrderService.Application.DTOs.Common;

using OrderService.Domain.Enums;

public class AddressDto
{
    public string Street { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Pincode { get; set; } = string.Empty;

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    public AddressType AddressType { get; set; } = AddressType.Home;
}