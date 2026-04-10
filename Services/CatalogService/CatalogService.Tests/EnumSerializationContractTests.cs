using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;
using CatalogService.Application.DTOs.Restaurant;
using CatalogService.Application.DTOs.MenuItem;
using CatalogService.Domain.Enums;

namespace CatalogService.Tests;

/// <summary>
/// Tests for JSON serialization of CatalogService DTOs
/// </summary>
[TestFixture]
public class EnumSerializationContractTests
{
    private JsonSerializerOptions _jsonOptions;

    [SetUp]
    public void SetUp()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter(allowIntegerValues: true) }
        };
    }

    [Test]
    public void CuisineType_SerializesAsText()
    {
        var cuisine = CuisineType.Italian;
        var json = JsonSerializer.Serialize(cuisine, _jsonOptions);
        Assert.That(json, Is.EqualTo(@"""Italian"""));
    }

    [Test]
    public void RestaurantStatus_SerializesAsText()
    {
        var status = RestaurantStatus.Active;
        var json = JsonSerializer.Serialize(status, _jsonOptions);
        Assert.That(json, Is.EqualTo(@"""Active"""));
    }

    [Test]
    public void ItemAvailabilityStatus_DeserializesFromText()
    {
        var json = @"""Available""";
        var result = JsonSerializer.Deserialize<ItemAvailabilityStatus>(json, _jsonOptions);
        Assert.That(result, Is.EqualTo(ItemAvailabilityStatus.Available));
    }

    [Test]
    public void EnumConverter_BackwardCompatibilityWithNumeric()
    {
        var json = @"1";
        var result = JsonSerializer.Deserialize<CuisineType>(json, _jsonOptions);
        Assert.That(result, Is.EqualTo(CuisineType.Italian));
    }

    [Test]
    public void RestaurantDto_RoundTripPreservesEnums()
    {
        var original = new RestaurantDto
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            CuisineType = CuisineType.Chinese,
            Status = RestaurantStatus.Suspended
        };

        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<RestaurantDto>(json, _jsonOptions);

        Assert.That(deserialized.CuisineType, Is.EqualTo(original.CuisineType));
        Assert.That(deserialized.Status, Is.EqualTo(original.Status));
    }
}
