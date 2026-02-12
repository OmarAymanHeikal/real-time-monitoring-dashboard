namespace MonitoringDashboard.Application.Common.Interfaces;

public interface IReportFileService
{
    Task<string> WriteReportAsync(int reportId, int serverId, DateTime start, DateTime end, object reportData, CancellationToken cancellationToken = default);
    Task<Stream?> GetReportStreamAsync(string filePath, CancellationToken cancellationToken = default);
}
