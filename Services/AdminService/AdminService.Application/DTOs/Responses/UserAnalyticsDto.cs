namespace AdminService.Application.DTOs.Responses;

public class UserAnalyticsDto
{
    public int TotalUsersRegistered { get; set; }
    public int ActiveUsers { get; set; }
    public Dictionary<string, int> UsersByRole { get; set; } = new();
    public List<RegistrationTrendDto> RegistrationTrend { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class RegistrationTrendDto
{
    public DateTime Date { get; set; }
    public int NewRegistrations { get; set; }
}
