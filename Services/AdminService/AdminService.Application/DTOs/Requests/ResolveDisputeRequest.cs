namespace AdminService.Application.DTOs.Requests;

public class ResolveDisputeRequest
{
    public string Resolution { get; set; } = string.Empty;
    public string ResolutionNotes { get; set; } = string.Empty;
    public decimal? RefundAmount { get; set; }
}
