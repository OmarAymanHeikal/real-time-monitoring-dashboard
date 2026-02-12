namespace MonitoringDashboard.Domain.Enums;

/// <summary>
/// Lifecycle status of a report generation job.
/// </summary>
public enum ReportStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}
