using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDashboard.Application.Features.Alerts.ListAlertsQuery;
using MonitoringDashboard.Application.Features.Alerts.ResolveAlertCommand;
using Asp.Versioning;

namespace MonitoringDashboard.WebAPI.Controllers.Api.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class AlertsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AlertsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<object>> List([FromQuery] int? serverId, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ListAlertsQuery(serverId, status, page, pageSize), ct);
        return Ok(result);
    }

    [HttpPost("{id:int}/resolve")]
    public async Task<ActionResult> Resolve(int id, CancellationToken ct)
    {
        await _mediator.Send(new ResolveAlertCommand(id), ct);
        return NoContent();
    }
}
