using OrderService.Application.Exceptions;
using OrderService.Domain.Exceptions;

namespace OrderService.API.Middleware;

public class GlobalExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred in OrderService API.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = StatusCodes.Status500InternalServerError;
        var message = "An unexpected error occurred. Please try again later.";

        switch (exception)
        {
            case ResourceNotFoundException:
                statusCode = StatusCodes.Status404NotFound;
                message = exception.Message;
                break;
            case ValidationException:
            case CartException:
            case OrderException:
            case PaymentException:
            case ArgumentException:
            case InvalidOperationException:
                statusCode = StatusCodes.Status400BadRequest;
                message = exception.Message;
                break;
        }

        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsJsonAsync(new
        {
            statusCode = statusCode,
            message = message,
            timestamp = DateTime.UtcNow
        });
    }
}

public sealed class ErrorResponse
{
    public int StatusCode { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }
}
