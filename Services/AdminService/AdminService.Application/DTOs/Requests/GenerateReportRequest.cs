namespace AdminService.Application.DTOs.Requests;

public class GenerateReportRequest
{
    public string ReportType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid? RestaurantId { get; set; }
    public string? FilterCriteria { get; set; }
}
