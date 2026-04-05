using AdminService.Domain.Enums;
using AdminService.Domain.ValueObjects;
using AdminService.Domain.Events;

namespace AdminService.Domain.Entities;

/// <summary>
/// Report aggregate root representing generated reports
/// </summary>
public class Report
{
    public Guid Id { get; private set; }
    public ReportType Type { get; private set; }
    public ReportMetrics Metrics { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public DateTime GeneratedAt { get; private set; }
    public string? FilterCriteria { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Report() 
    {
        Metrics = null!;
    } // For EF Core

    private Report(ReportType type, ReportMetrics metrics, DateTime startDate, DateTime endDate, string? filterCriteria = null)
    {
        Id = Guid.NewGuid();
        Type = type;
        Metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        StartDate = startDate;
        EndDate = endDate;
        FilterCriteria = filterCriteria;
        GeneratedAt = DateTime.UtcNow;
    }

    public static Report Create(ReportType type, ReportMetrics metrics, DateTime startDate, DateTime endDate, string? filterCriteria = null)
    {
        var report = new Report(type, metrics, startDate, endDate, filterCriteria);
        report.AddDomainEvent(new ReportGeneratedEvent(
            report.Id,
            report.Type,
            report.StartDate,
            report.EndDate,
            report.Metrics.TotalOrders
        ));
        return report;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
