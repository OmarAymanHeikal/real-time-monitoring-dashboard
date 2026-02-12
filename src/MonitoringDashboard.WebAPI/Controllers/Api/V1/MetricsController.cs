using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDashboard.Application.Features.Metrics.ListMetricsQuery;
using Asp.Versioning;

namespace MonitoringDashboard.WebAPI.Controllers.Api.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class MetricsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MetricsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("server/{serverId:int}")]
    public async Task<IActionResult> List(int serverId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int page = 1, [FromQuery] int pageSize = 100, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ListMetricsQuery(serverId, from, to, page, pageSize), ct);
        return Ok(result);
    }
}
