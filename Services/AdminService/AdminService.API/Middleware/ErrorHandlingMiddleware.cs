using System.Net;
using System.Text.Json;
using FluentValidation;

namespace AdminService.API.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        var result = string.Empty;

        switch (exception)
        {
            case ValidationException validationException:
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(new
                {
                    error = "Validation failed",
                    errors = validationException.Errors.Select(e => new
                    {
                        property = e.PropertyName,
                        message = e.ErrorMessage
                    })
                });
                break;

            case KeyNotFoundException:
                code = HttpStatusCode.NotFound;
                result = JsonSerializer.Serialize(new { error = exception.Message });
                break;

            case UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                result = JsonSerializer.Serialize(new { error = "Unauthorized access" });
                break;

            case ArgumentException:
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(new { error = exception.Message });
                break;

            case InvalidOperationException:
                code = HttpStatusCode.BadRequest;
                result = JsonSerializer.Serialize(new { error = exception.Message });
                break;

            default:
                result = JsonSerializer.Serialize(new
                {
                    error = "An internal server error occurred",
                    message = exception.Message
                });
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;
        return context.Response.WriteAsync(result);
    }
}
