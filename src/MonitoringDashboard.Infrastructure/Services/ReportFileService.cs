using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MonitoringDashboard.Application.Common.Interfaces;

namespace MonitoringDashboard.Infrastructure.Services;

public class ReportFileService : IReportFileService
{
    private readonly string _basePath;
    private readonly ILogger<ReportFileService> _logger;

    public ReportFileService(IConfiguration config, ILogger<ReportFileService> logger)
    {
        _basePath = config["Reports:BasePath"] ?? Path.Combine(Path.GetTempPath(), "MonitoringReports");
        _logger = logger;
        if (!Directory.Exists(_basePath)) Directory.CreateDirectory(_basePath);
    }

    public async Task<string> WriteReportAsync(int reportId, int serverId, DateTime start, DateTime end, object reportData, CancellationToken cancellationToken = default)
    {
        var fileName = $"report_{reportId}_{serverId}_{start:yyyyMMddHHmmss}_{end:yyyyMMddHHmmss}.html";
        var path = Path.Combine(_basePath, fileName);
        
        var html = GenerateHtmlReport(reportData, reportId, serverId, start, end);
        await File.WriteAllTextAsync(path, html, cancellationToken);
        
        _logger.LogInformation("Report file written: {Path}", path);
        return path;
    }

    private string GenerateHtmlReport(object reportData, int reportId, int serverId, DateTime start, DateTime end)
    {
        var json = JsonSerializer.Serialize(reportData, new JsonSerializerOptions { WriteIndented = true });
        var data = JsonSerializer.Deserialize<JsonElement>(json);
        
        var serverName = data.TryGetProperty("ServerName", out var sn) ? sn.GetString() : $"Server {serverId}";
        var metricsCount = data.TryGetProperty("MetricsCount", out var mc) ? mc.GetInt32() : 0;
        var generatedAt = data.TryGetProperty("GeneratedAt", out var ga) ? ga.GetDateTime() : DateTime.UtcNow;
        
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta charset=\"UTF-8\">");
        sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine($"    <title>Report #{reportId} - {serverName}</title>");
        sb.AppendLine("    <style>");
        sb.AppendLine("        * { margin: 0; padding: 0; box-sizing: border-box; }");
        sb.AppendLine("        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; padding: 40px; background: #f5f5f5; }");
        sb.AppendLine("        .container { max-width: 1200px; margin: 0 auto; background: white; padding: 40px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); }");
        sb.AppendLine("        .header { border-bottom: 3px solid #3f51b5; padding-bottom: 20px; margin-bottom: 30px; }");
        sb.AppendLine("        .header h1 { color: #3f51b5; font-size: 32px; margin-bottom: 10px; }");
        sb.AppendLine("        .header .subtitle { color: #666; font-size: 18px; }");
        sb.AppendLine("        .info-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 20px; margin: 30px 0; }");
        sb.AppendLine("        .info-card { background: #f8f9fa; padding: 20px; border-radius: 6px; border-left: 4px solid #3f51b5; }");
        sb.AppendLine("        .info-card .label { font-size: 12px; color: #666; text-transform: uppercase; margin-bottom: 5px; }");
        sb.AppendLine("        .info-card .value { font-size: 20px; font-weight: 600; color: #333; }");
        sb.AppendLine("        .section { margin: 40px 0; }");
        sb.AppendLine("        .section h2 { color: #333; font-size: 24px; margin-bottom: 20px; padding-bottom: 10px; border-bottom: 2px solid #e0e0e0; }");
        sb.AppendLine("        .summary-stats { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 15px; margin: 20px 0; }");
        sb.AppendLine("        .stat-box { padding: 15px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; border-radius: 6px; text-align: center; }");
        sb.AppendLine("        .stat-box .stat-label { font-size: 14px; opacity: 0.9; margin-bottom: 5px; }");
        sb.AppendLine("        .stat-box .stat-value { font-size: 28px; font-weight: bold; }");
        sb.AppendLine("        table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
        sb.AppendLine("        thead { background: #3f51b5; color: white; }");
        sb.AppendLine("        th, td { padding: 12px; text-align: left; border-bottom: 1px solid #e0e0e0; }");
        sb.AppendLine("        th { font-weight: 600; }");
        sb.AppendLine("        tbody tr:hover { background: #f5f5f5; }");
        sb.AppendLine("        .status-online { color: #4caf50; font-weight: 600; }");
        sb.AppendLine("        .status-offline { color: #f44336; font-weight: 600; }");
        sb.AppendLine("        .status-degraded { color: #ff9800; font-weight: 600; }");
        sb.AppendLine("        .metric-good { color: #4caf50; }");
        sb.AppendLine("        .metric-warning { color: #ff9800; }");
        sb.AppendLine("        .metric-critical { color: #f44336; }");
        sb.AppendLine("        .footer { margin-top: 40px; padding-top: 20px; border-top: 2px solid #e0e0e0; text-align: center; color: #666; font-size: 14px; }");
        sb.AppendLine("        @media print { body { padding: 0; background: white; } .container { box-shadow: none; } }");
        sb.AppendLine("    </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("    <div class=\"container\">");
        sb.AppendLine("        <div class=\"header\">");
        sb.AppendLine($"            <h1>Server Monitoring Report</h1>");
        sb.AppendLine($"            <div class=\"subtitle\">Report #{reportId} - {serverName}</div>");
        sb.AppendLine("        </div>");
        
        sb.AppendLine("        <div class=\"info-grid\">");
        sb.AppendLine("            <div class=\"info-card\">");
        sb.AppendLine("                <div class=\"label\">Report ID</div>");
        sb.AppendLine($"                <div class=\"value\">#{reportId}</div>");
        sb.AppendLine("            </div>");
        sb.AppendLine("            <div class=\"info-card\">");
        sb.AppendLine("                <div class=\"label\">Server</div>");
        sb.AppendLine($"                <div class=\"value\">{serverName}</div>");
        sb.AppendLine("            </div>");
        sb.AppendLine("            <div class=\"info-card\">");
        sb.AppendLine("                <div class=\"label\">Period</div>");
        sb.AppendLine($"                <div class=\"value\">{start:MMM dd, HH:mm} - {end:MMM dd, HH:mm}</div>");
        sb.AppendLine("            </div>");
        sb.AppendLine("            <div class=\"info-card\">");
        sb.AppendLine("                <div class=\"label\">Generated</div>");
        sb.AppendLine($"                <div class=\"value\">{generatedAt:MMM dd, yyyy HH:mm}</div>");
        sb.AppendLine("            </div>");
        sb.AppendLine("        </div>");

        // Calculate statistics
        if (data.TryGetProperty("Metrics", out var metricsElement) && metricsElement.ValueKind == JsonValueKind.Array)
        {
            var metrics = metricsElement.EnumerateArray().ToList();
            if (metrics.Any())
            {
                var avgCpu = metrics.Average(m => m.TryGetProperty("CpuUsage", out var cpu) ? cpu.GetDouble() : 0);
                var avgMem = metrics.Average(m => m.TryGetProperty("MemoryUsage", out var mem) ? mem.GetDouble() : 0);
                var avgDisk = metrics.Average(m => m.TryGetProperty("DiskUsage", out var disk) ? disk.GetDouble() : 0);
                var avgResponse = metrics.Average(m => m.TryGetProperty("ResponseTime", out var res) ? res.GetDouble() : 0);

                sb.AppendLine("        <div class=\"section\">");
                sb.AppendLine("            <h2>Summary Statistics</h2>");
                sb.AppendLine("            <div class=\"summary-stats\">");
                sb.AppendLine("                <div class=\"stat-box\">");
                sb.AppendLine("                    <div class=\"stat-label\">Avg CPU Usage</div>");
                sb.AppendLine($"                    <div class=\"stat-value\">{avgCpu:F1}%</div>");
                sb.AppendLine("                </div>");
                sb.AppendLine("                <div class=\"stat-box\" style=\"background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);\">");
                sb.AppendLine("                    <div class=\"stat-label\">Avg Memory Usage</div>");
                sb.AppendLine($"                    <div class=\"stat-value\">{avgMem:F1}%</div>");
                sb.AppendLine("                </div>");
                sb.AppendLine("                <div class=\"stat-box\" style=\"background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%);\">");
                sb.AppendLine("                    <div class=\"stat-label\">Avg Disk Usage</div>");
                sb.AppendLine($"                    <div class=\"stat-value\">{avgDisk:F1}%</div>");
                sb.AppendLine("                </div>");
                sb.AppendLine("                <div class=\"stat-box\" style=\"background: linear-gradient(135deg, #43e97b 0%, #38f9d7 100%);\">");
                sb.AppendLine("                    <div class=\"stat-label\">Avg Response Time</div>");
                sb.AppendLine($"                    <div class=\"stat-value\">{avgResponse:F0}ms</div>");
                sb.AppendLine("                </div>");
                sb.AppendLine("                <div class=\"stat-box\" style=\"background: linear-gradient(135deg, #fa709a 0%, #fee140 100%);\">");
                sb.AppendLine("                    <div class=\"stat-label\">Total Data Points</div>");
                sb.AppendLine($"                    <div class=\"stat-value\">{metricsCount}</div>");
                sb.AppendLine("                </div>");
                sb.AppendLine("            </div>");
                sb.AppendLine("        </div>");

                sb.AppendLine("        <div class=\"section\">");
                sb.AppendLine("            <h2>Detailed Metrics</h2>");
                sb.AppendLine("            <table>");
                sb.AppendLine("                <thead>");
                sb.AppendLine("                    <tr>");
                sb.AppendLine("                        <th>Timestamp</th>");
                sb.AppendLine("                        <th>CPU Usage</th>");
                sb.AppendLine("                        <th>Memory Usage</th>");
                sb.AppendLine("                        <th>Disk Usage</th>");
                sb.AppendLine("                        <th>Response Time</th>");
                sb.AppendLine("                        <th>Status</th>");
                sb.AppendLine("                    </tr>");
                sb.AppendLine("                </thead>");
                sb.AppendLine("                <tbody>");

                foreach (var metric in metrics)
                {
                    var timestamp = metric.TryGetProperty("Timestamp", out var ts) ? ts.GetDateTime() : DateTime.MinValue;
                    var cpu = metric.TryGetProperty("CpuUsage", out var c) ? c.GetDouble() : 0;
                    var mem = metric.TryGetProperty("MemoryUsage", out var m) ? m.GetDouble() : 0;
                    var disk = metric.TryGetProperty("DiskUsage", out var d) ? d.GetDouble() : 0;
                    var response = metric.TryGetProperty("ResponseTime", out var r) ? r.GetDouble() : 0;
                    var status = metric.TryGetProperty("Status", out var s) ? s.GetString() : "Unknown";

                    var cpuClass = cpu > 80 ? "metric-critical" : cpu > 60 ? "metric-warning" : "metric-good";
                    var memClass = mem > 80 ? "metric-critical" : mem > 60 ? "metric-warning" : "metric-good";
                    var diskClass = disk > 80 ? "metric-critical" : disk > 60 ? "metric-warning" : "metric-good";
                    var statusClass = status?.ToLower() == "online" ? "status-online" : status?.ToLower() == "offline" ? "status-offline" : "status-degraded";

                    sb.AppendLine("                    <tr>");
                    sb.AppendLine($"                        <td>{timestamp:MMM dd, HH:mm:ss}</td>");
                    sb.AppendLine($"                        <td class=\"{cpuClass}\">{cpu:F1}%</td>");
                    sb.AppendLine($"                        <td class=\"{memClass}\">{mem:F1}%</td>");
                    sb.AppendLine($"                        <td class=\"{diskClass}\">{disk:F1}%</td>");
                    sb.AppendLine($"                        <td>{response:F0}ms</td>");
                    sb.AppendLine($"                        <td class=\"{statusClass}\">{status}</td>");
                    sb.AppendLine("                    </tr>");
                }

                sb.AppendLine("                </tbody>");
                sb.AppendLine("            </table>");
                sb.AppendLine("        </div>");
            }
        }

        sb.AppendLine("        <div class=\"footer\">");
        sb.AppendLine($"            <p>Generated by Monitoring Dashboard on {generatedAt:MMMM dd, yyyy 'at' HH:mm:ss}</p>");
        sb.AppendLine("            <p>This report contains confidential information. Handle with care.</p>");
        sb.AppendLine("        </div>");
        sb.AppendLine("    </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    public Task<Stream?> GetReportStreamAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return Task.FromResult<Stream?>(null);
        try
        {
            return Task.FromResult<Stream?>(File.OpenRead(filePath));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not open report file: {Path}", filePath);
            return Task.FromResult<Stream?>(null);
        }
    }
}
