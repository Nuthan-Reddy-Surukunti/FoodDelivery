using CatalogService.Application.Exceptions;
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
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Timestamp = DateTime.UtcNow
        };

        switch (exception)
        {
            case RestaurantNotFoundException ex:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response.StatusCode = 404;
                response.Message = ex.Message;
                break;

            case MenuItemNotFoundException ex:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response.StatusCode = 404;
                response.Message = ex.Message;
                break;

            case CategoryNotFoundException ex:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response.StatusCode = 404;
                response.Message = ex.Message;
                break;

            case DuplicateCategoryException ex:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                response.StatusCode = 409;
                response.Message = ex.Message;
                break;

            case InvalidMenuItemPriceException ex:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.StatusCode = 400;
                response.Message = ex.Message;
                break;

            case InvalidRestaurantDataException ex:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.StatusCode = 400;
                response.Message = ex.Message;
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response.StatusCode = 500;
                response.Message = "An unexpected error occurred. Please try again later.";
                break;
        }

        return context.Response.WriteAsJsonAsync(response);
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
