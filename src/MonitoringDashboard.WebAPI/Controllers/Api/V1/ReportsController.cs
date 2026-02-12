using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDashboard.Application.Common.Interfaces;
using MonitoringDashboard.Application.Features.Reports.GetReportQuery;
using MonitoringDashboard.Application.Features.Reports.ListReportsQuery;
using MonitoringDashboard.Application.Features.Reports.RequestReportCommand;
using MonitoringDashboard.Application.Features.Reports.DeleteReportCommand;
using Hangfire;
using MonitoringDashboard.Infrastructure.BackgroundJobs;
using Asp.Versioning;

namespace MonitoringDashboard.WebAPI.Controllers.Api.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IReportFileService _reportFile;
    private readonly IBackgroundJobClient _hangfire;

    public ReportsController(IMediator mediator, IReportFileService reportFile, IBackgroundJobClient hangfire)
    {
        _mediator = mediator;
        _reportFile = reportFile;
        _hangfire = hangfire;
    }

    [HttpGet]
    public async Task<ActionResult<object>> List([FromQuery] int? serverId, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ListReportsQuery(serverId, status, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReportDto>> Get(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetReportQuery(id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("request")]
    public async Task<ActionResult<object>> RequestReport([FromBody] RequestReportCommand command, CancellationToken ct)
    {
        var reportId = await _mediator.Send(command, ct);
        
        // Enqueue report generation job and chain cleanup as continuation
        var parentJobId = _hangfire.Enqueue<ReportGenerationJob>(j => j.ExecuteAsync(reportId));
        _hangfire.ContinueJobWith<ReportCleanupJob>(parentJobId, j => j.ExecuteAsync(reportId));
        
        return AcceptedAtAction(nameof(Get), new { id = reportId }, new { reportId });
    }

    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(int id, CancellationToken ct)
    {
        var report = await _mediator.Send(new GetReportQuery(id), ct);
        if (report == null || string.IsNullOrEmpty(report.FilePath)) return NotFound();
        var stream = await _reportFile.GetReportStreamAsync(report.FilePath, ct);
        if (stream == null) return NotFound();
        var fileName = Path.GetFileName(report.FilePath);
        var contentType = fileName.EndsWith(".html") ? "text/html" : "application/json";
        return File(stream, contentType, fileName);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await _mediator.Send(new DeleteReportCommand(id), ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
