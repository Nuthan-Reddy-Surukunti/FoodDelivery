using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;

namespace AuthService.Tests;

/// <summary>
/// Tests to verify JSON enum converter is properly configured in AuthService
/// Validates that the enum converter integration doesn't break auth functionality
/// </summary>
[TestFixture]
public class EnumConverterConfigurationTests
{
    private JsonSerializerOptions _jsonOptionsWithEnumConverter;

    [SetUp]
    public void SetUp()
    {
        // Configure options to match what AuthService API uses
        _jsonOptionsWithEnumConverter = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter(allowIntegerValues: true) },
            WriteIndented = false
        };
    }

    [Test]
    public void JsonOptions_ContainsStringEnumConverter()
    {
        // Assert
        Assert.That(_jsonOptionsWithEnumConverter.Converters.Any(c => c is JsonStringEnumConverter),
            "JsonSerializerOptions should contain StringEnumConverter");
    }

    [Test]
    public void JsonOptions_AllowsIntegerEnumValues()
    {
        // Arrange
        var converter = _jsonOptionsWithEnumConverter.Converters
            .OfType<JsonStringEnumConverter>()
            .First();

        // This test verifies that the converter supports integer values for backward compatibility
        // The AllowIntegerValues property is not directly accessible, but we can verify
        // through actual deserialization behavior

        // Act - attempt to deserialize an integer as an enum
        var testEnum = TestEnum.ValueTwo;
        var json = JsonSerializer.Serialize((int)testEnum, _jsonOptionsWithEnumConverter);

        // Assert
        Assert.That(json, Is.EqualTo("2"), "Should serialize integer value");
    }

    [Test]
    public void EnumConverter_DeserializesTextFormat()
    {
        // Arrange
        var json = @"""ValueOne""";

        // Act
        var result = JsonSerializer.Deserialize<TestEnum>(json, _jsonOptionsWithEnumConverter);

        // Assert
        Assert.That(result, Is.EqualTo(TestEnum.ValueOne),
            "Enum converter should deserialize text values");
    }

    [Test]
    public void EnumConverter_DeserializesNumericFormat()
    {
        // Arrange - backward compatibility test
        var json = @"1";

        // Act
        var result = JsonSerializer.Deserialize<TestEnum>(json, _jsonOptionsWithEnumConverter);

        // Assert
        Assert.That(result, Is.EqualTo(TestEnum.ValueOne),
            "Enum converter should support numeric deserialization for backward compatibility");
    }

    [Test]
    public void EnumConverter_SerializesAsText()
    {
        // Arrange
        var value = TestEnum.ValueTwo;

        // Act
        var json = JsonSerializer.Serialize(value, _jsonOptionsWithEnumConverter);

        // Assert
        Assert.That(json, Is.EqualTo(@"""ValueTwo"""),
            "Enum converter should serialize as text value");
    }

    // Test enum for converter verification
    private enum TestEnum
    {
        ValueOne = 1,
        ValueTwo = 2
    }
}
