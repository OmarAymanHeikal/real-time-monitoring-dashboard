using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDashboard.Application.Common.Models;
using MonitoringDashboard.Application.Features.Servers.CreateServerCommand;
using MonitoringDashboard.Application.Features.Servers.DeleteServerCommand;
using MonitoringDashboard.Application.Features.Servers.GetServerQuery;
using MonitoringDashboard.Application.Features.Servers.ListServersQuery;
using MonitoringDashboard.Application.Features.Servers.UpdateServerCommand;
using Asp.Versioning;
using Hangfire;
using MonitoringDashboard.Infrastructure.BackgroundJobs;

namespace MonitoringDashboard.WebAPI.Controllers.Api.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ServersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public ServersController(IMediator mediator, IBackgroundJobClient backgroundJobClient)
    {
        _mediator = mediator;
        _backgroundJobClient = backgroundJobClient;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ServerListItemDto>>> List([FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ListServersQuery(search, status, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServerDto>> Get(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetServerQuery(id), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServerCommand command, CancellationToken ct)
    {
        var id = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(Get), new { id }, new { serverId = id });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateServerRequest request, CancellationToken ct)
    {
        await _mediator.Send(new UpdateServerCommand(id, request.Name, request.IPAddress, request.Description, request.Status), ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteServerCommand(id), ct);
        return NoContent();
    }

    /// <summary>
    /// Demonstrates Hangfire delayed job: Schedule maintenance notification
    /// </summary>
    [HttpPost("{id:int}/schedule-maintenance")]
    public async Task<IActionResult> ScheduleMaintenance(int id, [FromBody] ScheduleMaintenanceRequest request, CancellationToken ct)
    {
        // Verify server exists
        var server = await _mediator.Send(new GetServerQuery(id), ct);
        if (server == null) return NotFound(new { error = "Server not found" });

        // Calculate delay until maintenance time
        var delay = request.MaintenanceTime - DateTime.UtcNow;
        if (delay.TotalSeconds < 0)
        {
            return BadRequest(new { error = "Maintenance time must be in the future" });
        }

        // Schedule delayed job using Hangfire
        var jobId = _backgroundJobClient.Schedule<MaintenanceNotificationJob>(
            job => job.ExecuteAsync(id, request.MaintenanceTime),
            delay
        );

        return Ok(new
        {
            jobId,
            serverId = id,
            serverName = server.Name,
            maintenanceTime = request.MaintenanceTime,
            scheduledDelay = $"{delay.TotalMinutes:F1} minutes",
            message = "Maintenance notification scheduled successfully"
        });
    }
}

public record UpdateServerRequest(string Name, string? IPAddress, string? Description, string? Status);

public record ScheduleMaintenanceRequest(DateTime MaintenanceTime);
