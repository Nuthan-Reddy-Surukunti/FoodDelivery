using System.Net;
using System.Text.Json;
using FluentValidation;
using QuickBite.Shared.Contracts;

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
            _logger.LogError(ex,
                "Unhandled exception in AdminService API for {Method} {Path}. TraceId={TraceId}",
                context.Request.Method,
                context.Request.Path.Value,
                context.TraceIdentifier);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = exception switch
        {
            ValidationException validationException => new ApiErrorResponse
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Validation Failed",
                Detail = "One or more validation errors occurred.",
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                ErrorCode = "VALIDATION_ERROR",
                Errors = validationException.Errors.Select(e => new
                {
                    property = e.PropertyName,
                    message = e.ErrorMessage
                }).ToArray()
            },
            KeyNotFoundException => new ApiErrorResponse
            {
                Status = (int)HttpStatusCode.NotFound,
                Title = "Not Found",
                Detail = "The requested resource was not found.",
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                ErrorCode = "NOT_FOUND"
            },
            UnauthorizedAccessException => new ApiErrorResponse
            {
                Status = (int)HttpStatusCode.Forbidden,
                Title = "Forbidden",
                Detail = "You do not have permission to perform this action.",
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                ErrorCode = "FORBIDDEN"
            },
            ArgumentException => new ApiErrorResponse
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Bad Request",
                Detail = "The request could not be processed.",
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                ErrorCode = "BAD_REQUEST"
            },
            InvalidOperationException => new ApiErrorResponse
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Bad Request",
                Detail = "The operation could not be completed.",
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                ErrorCode = "BAD_REQUEST"
            },
            _ => new ApiErrorResponse
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred.",
                TraceId = context.TraceIdentifier,
                Timestamp = DateTime.UtcNow,
                ErrorCode = "INTERNAL_ERROR"
            }
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = response.Status;
        return context.Response.WriteAsJsonAsync(response);
    }
}
