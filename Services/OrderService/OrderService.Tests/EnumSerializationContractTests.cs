using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;
using OrderService.Application.DTOs.Order;
using OrderService.Domain.Enums;

namespace OrderService.Tests;

/// <summary>
/// Tests for JSON serialization and deserialization of OrderService enums
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
            Converters = { new JsonStringEnumConverter(allowIntegerValues: true) }
        };
    }

    [Test]
    public void OrderStatus_SerializesAsText()
    {
        var json = JsonSerializer.Serialize(OrderStatus.DraftCart, _jsonOptions);
        Assert.That(json, Is.EqualTo(@"""DraftCart"""));
    }

    [Test]
    public void OrderStatus_DeserializesFromText()
    {
        var result = JsonSerializer.Deserialize<OrderStatus>(@"""DraftCart""", _jsonOptions);
        Assert.That(result, Is.EqualTo(OrderStatus.DraftCart));
    }

    [Test]
    public void OrderStatus_DeserializesFromNumericForBackwardCompatibility()
    {
        var result = JsonSerializer.Deserialize<OrderStatus>(@"1", _jsonOptions);
        Assert.That(result, Is.EqualTo(OrderStatus.DraftCart));
    }

    [Test]
    public void PaymentStatus_SerializesAsText()
    {
        var json = JsonSerializer.Serialize(PaymentStatus.Success, _jsonOptions);
        Assert.That(json, Is.EqualTo(@"""Success"""));
    }

    [Test]
    public void PaymentMethod_SerializesAsText()
    {
        var json = JsonSerializer.Serialize(PaymentMethod.Online, _jsonOptions);
        Assert.That(json, Is.EqualTo(@"""Online"""));
    }

    [Test]
    public void PaymentMethod_DeserializesFromNumericForBackwardCompatibility()
    {
        var result = JsonSerializer.Deserialize<PaymentMethod>(@"2", _jsonOptions);
        Assert.That(result, Is.EqualTo(PaymentMethod.CashOnDelivery));
    }

    [Test]
    public void OrderDetailDto_RoundTripPreservesEnumValues()
    {
        var original = new OrderDetailDto
        {
            OrderId = Guid.NewGuid(),
            OrderStatus = OrderStatus.Delivered,
            RestaurantId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };

        var json = JsonSerializer.Serialize(original, _jsonOptions);
        var deserialized = JsonSerializer.Deserialize<OrderDetailDto>(json, _jsonOptions);

        Assert.That(deserialized.OrderStatus, Is.EqualTo(original.OrderStatus));
    }
}
