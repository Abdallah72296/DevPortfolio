using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MediatR;
using MyPortfolio.Application.AdminUsers;
using MyPortfolio.Application.AdminUsers.Models;

namespace MyPortfolio.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/users")]
public sealed class AdminUsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminUsersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? role = null,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAdminUsersQuery(pageNumber, pageSize, search, role, status), cancellationToken);
        if (!result.Succeeded || result.Data is null) return BadRequest(result);
        return Ok(new
        {
            succeeded = true,
            data = result.Data.Items,
            message = result.Message,
            errors = Array.Empty<string>(),
            pagination = new { result.Data.PageNumber, result.Data.PageSize, result.Data.TotalCount, result.Data.TotalPages },
            summary = result.Data.Summary
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAdminUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateAdminUserCommand(request), cancellationToken);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id}/role")]
    public async Task<IActionResult> SetRole(string id, [FromBody] SetAdminUserRoleRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new SetAdminUserRoleCommand(id, request.Role), cancellationToken);
        return ToActionResult(result);
    }

    [HttpPut("{id}/lock")]
    public async Task<IActionResult> SetLock(string id, [FromBody] SetAdminUserLockRequest request, CancellationToken cancellationToken = default)
    {
        var currentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _mediator.Send(new SetAdminUserLockCommand(id, request.Locked, currentId), cancellationToken);
        return ToActionResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken = default)
    {
        var currentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _mediator.Send(new DeleteAdminUserCommand(id, currentId), cancellationToken);
        if (result.Succeeded) return NoContent();
        if (result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true) return NotFound(result);
        if (result.Message?.Contains("cannot", StringComparison.OrdinalIgnoreCase) == true || result.Message?.Contains("last administrator", StringComparison.OrdinalIgnoreCase) == true)
            return Conflict(result);
        return BadRequest(result);
    }

    private IActionResult ToActionResult(MyPortfolio.Application.Common.Models.Result<AdminUserDto> result)
    {
        if (result.Succeeded) return Ok(result);
        if (result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true) return NotFound(result);
        if (result.Message?.Contains("last administrator", StringComparison.OrdinalIgnoreCase) == true || result.Message?.Contains("cannot", StringComparison.OrdinalIgnoreCase) == true)
            return Conflict(result);
        return BadRequest(result);
    }
}
