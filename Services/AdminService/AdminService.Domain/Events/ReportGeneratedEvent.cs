using AdminService.Domain.Enums;

namespace AdminService.Domain.Events;

/// <summary>
/// Event raised when a report is generated
/// </summary>
public sealed class ReportGeneratedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    
    public Guid ReportId { get; }
    public ReportType ReportType { get; }
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    public int RecordCount { get; }

    public ReportGeneratedEvent(
        Guid reportId, 
        ReportType reportType, 
        DateTime startDate, 
        DateTime endDate,
        int recordCount)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        ReportId = reportId;
        ReportType = reportType;
        StartDate = startDate;
        EndDate = endDate;
        RecordCount = recordCount;
    }
}
