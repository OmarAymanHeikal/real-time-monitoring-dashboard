using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonitoringDashboard.Application.Features.Auth.LoginCommand;
using MonitoringDashboard.Application.Features.Auth.RegisterCommand;
using MonitoringDashboard.Application.Features.Auth.RefreshTokenCommand;
using Asp.Versioning;

namespace MonitoringDashboard.WebAPI.Controllers.Api.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.Success) return BadRequest(new { error = result.Error });
        return Ok(new { result.AccessToken, result.RefreshToken, result.Role });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.Success) return BadRequest(new { error = result.Error });
        return Ok(new { userId = result.UserId });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.Success) return BadRequest(new { error = result.Error });
        return Ok(new { result.AccessToken, result.NewRefreshToken });
    }
}
