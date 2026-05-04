using CatalogService.Application.Exceptions;
using QuickBite.Shared.Contracts;
using System.Text.Json;

namespace CatalogService.API.Middleware;

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
            _logger.LogError(ex,
                "Unhandled exception in CatalogService API for {Method} {Path}. TraceId={TraceId}",
                context.Request.Method,
                context.Request.Path.Value,
                context.TraceIdentifier);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (status, title, detail, errorCode) = exception switch
        {
            RestaurantNotFoundException or MenuItemNotFoundException or CategoryNotFoundException
                => (StatusCodes.Status404NotFound, "Not Found", exception.Message, "NOT_FOUND"),
            DuplicateCategoryException
                => (StatusCodes.Status409Conflict, "Conflict", exception.Message, "CONFLICT"),
            InvalidMenuItemPriceException or InvalidRestaurantDataException or ArgumentException
                => (StatusCodes.Status400BadRequest, "Bad Request", exception.Message, "BAD_REQUEST"),
            UnauthorizedAccessException
                => (StatusCodes.Status403Forbidden, "Forbidden", "You do not have permission to perform this action.", "FORBIDDEN"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.", "INTERNAL_ERROR")
        };

        var response = new ApiErrorResponse
        {
            Status = status,
            Title = title,
            Detail = detail,
            TraceId = context.TraceIdentifier,
            Timestamp = DateTime.UtcNow,
            ErrorCode = errorCode
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsJsonAsync(response);
    }
}
