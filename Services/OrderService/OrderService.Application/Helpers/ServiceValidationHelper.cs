namespace OrderService.Application.Helpers;

using OrderService.Application.Exceptions;

public static class ServiceValidationHelper
{
    public static void ValidateIdentity(Guid id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ValidationException($"{parameterName} is required.");
        }
    }

    public static void ValidateNotNull<T>(T? value, string parameterName)
        where T : class
    {
        if (value is null)
        {
            throw new ValidationException($"{parameterName} is required.");
        }
    }

    public static void ValidatePositive(decimal value, string parameterName)
    {
        if (value <= 0)
        {
            throw new ValidationException($"{parameterName} must be greater than 0.");
        }
    }

    public static void ValidateNotNullOrWhitespace(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ValidationException($"{parameterName} is required.");
        }
    }
}
