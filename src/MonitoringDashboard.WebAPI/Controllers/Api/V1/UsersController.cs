using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDashboard.Application.Features.Users.ListRolesQuery;
using MonitoringDashboard.Application.Features.Users.ListUsersQuery;
using Asp.Versioning;

namespace MonitoringDashboard.WebAPI.Controllers.Api.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<object>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new ListUsersQuery(page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("roles")]
    public async Task<ActionResult<IReadOnlyList<RoleDto>>> Roles(CancellationToken ct)
    {
        var result = await _mediator.Send(new ListRolesQuery(), ct);
        return Ok(result);
    }
}
