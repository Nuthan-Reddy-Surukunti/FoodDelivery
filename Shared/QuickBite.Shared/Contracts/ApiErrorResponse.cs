namespace QuickBite.Shared.Contracts;

public sealed class ApiErrorResponse
{
    public int Status { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Detail { get; set; } = string.Empty;

    public string TraceId { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; }

    public string ErrorCode { get; set; } = string.Empty;

    public object? Errors { get; set; }
}
