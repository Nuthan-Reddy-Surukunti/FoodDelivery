using System.Diagnostics;

namespace AdminService.API.Middleware;

public sealed class RequestLoggingMiddleware
{
    private static readonly HashSet<string> SkippedPrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health",
        "/favicon.ico",
        "/swagger"
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkip(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            LogRequest(context, stopwatch.ElapsedMilliseconds);
        }
    }

    private void LogRequest(HttpContext context, long elapsedMilliseconds)
    {
        var statusCode = context.Response.StatusCode;
        var level = statusCode >= 500
            ? LogLevel.Error
            : statusCode >= 400
                ? LogLevel.Warning
                : LogLevel.Information;

        _logger.Log(
            level,
            "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms. TraceId={TraceId}",
            context.Request.Method,
            context.Request.Path.Value,
            statusCode,
            elapsedMilliseconds,
            context.TraceIdentifier);
    }

    private static bool ShouldSkip(PathString path)
    {
        var value = path.Value ?? string.Empty;
        return SkippedPrefixes.Any(prefix => value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }
}
