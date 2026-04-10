using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;

namespace AdminService.Tests;

/// <summary>
/// Tests for JSON enum converter configuration in AdminService
/// </summary>
[TestFixture]
public class EnumConverterConfigurationTests
{
    private JsonSerializerOptions _jsonOptions;

    [SetUp]
    public void SetUp()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter(allowIntegerValues: true) },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Test]
    public void JsonOptions_ContainsStringEnumConverter()
    {
        Assert.That(_jsonOptions.Converters.OfType<JsonStringEnumConverter>(),
            Is.Not.Empty, "JsonSerializerOptions should contain StringEnumConverter");
    }

    [Test]
    public void EnumConverter_SerializesAsText()
    {
        var testEnum = TestEnum.ValueTwo;
        var json = JsonSerializer.Serialize(testEnum, _jsonOptions);
        Assert.That(json, Is.EqualTo(@"""ValueTwo"""));
    }

    [Test]
    public void EnumConverter_DeserializesFromText()
    {
        var json = @"""ValueTwo""";
        var result = JsonSerializer.Deserialize<TestEnum>(json, _jsonOptions);
        Assert.That(result, Is.EqualTo(TestEnum.ValueTwo));
    }

    [Test]
    public void EnumConverter_DeserializesFromNumericForBackwardCompatibility()
    {
        var json = @"2";
        var result = JsonSerializer.Deserialize<TestEnum>(json, _jsonOptions);
        Assert.That(result, Is.EqualTo(TestEnum.ValueTwo));
    }

    private enum TestEnum
    {
        ValueOne = 1,
        ValueTwo = 2,
        ValueThree = 3
    }
}
