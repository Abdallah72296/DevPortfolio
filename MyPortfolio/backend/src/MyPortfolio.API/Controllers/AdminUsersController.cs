using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;

namespace MyPortfolio.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/users")]
public sealed class AdminUsersController : ControllerBase
{
    private readonly UserManager<IdentityUser> _users;
    private readonly RoleManager<IdentityRole> _roles;
    private readonly IGenericRepository<MyPortfolio.Domain.Entities.UserProfile> _profileRepository;
    private readonly IUserDataCleanupService _cleanupService;

    public AdminUsersController(
        UserManager<IdentityUser> users,
        RoleManager<IdentityRole> roles,
        IGenericRepository<MyPortfolio.Domain.Entities.UserProfile> profileRepository,
        IUserDataCleanupService cleanupService)
    {
        _users = users;
        _roles = roles;
        _profileRepository = profileRepository;
        _cleanupService = cleanupService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _users.Users.OrderBy(user => user.Email).ToListAsync(cancellationToken);
        var profiles = await _profileRepository.Query().IgnoreQueryFilters().AsNoTracking().ToListAsync(cancellationToken);
        var result = new List<AdminUserDto>(users.Count);
        foreach (var user in users)
        {
            var profile = profiles.FirstOrDefault(item => item.UserId == user.Id);
            result.Add(ToDto(user, await _users.GetRolesAsync(user), profile));
        }
        return Ok(new { succeeded = true, data = result });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAdminUserRequest request)
    {
        var role = NormalizeRole(request.Role);
        if (role == null) return BadRequest(new { succeeded = false, message = "Role must be Admin or User." });
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { succeeded = false, message = "Email and password are required." });

        var user = new IdentityUser { UserName = request.Email.Trim(), Email = request.Email.Trim(), EmailConfirmed = false };
        var created = await _users.CreateAsync(user, request.Password);
        if (!created.Succeeded) return BadRequest(new { succeeded = false, errors = created.Errors.Select(error => error.Description) });
        var assigned = await _users.AddToRoleAsync(user, role);
        if (!assigned.Succeeded)
        {
            await _users.DeleteAsync(user);
            return BadRequest(new { succeeded = false, errors = assigned.Errors.Select(error => error.Description) });
        }
        var fullName = string.IsNullOrWhiteSpace(request.FullName) ? request.Email.Trim().Split('@')[0] : request.FullName.Trim();
        var profile = new MyPortfolio.Domain.Entities.UserProfile(user.Id, fullName, "Developer", "Developer", "Portfolio",
            string.Empty, user.Email!, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "", "[]", false);
        profile.AssignToUser(user.Id);
        await _profileRepository.AddAsync(profile);
        await _profileRepository.SaveChangesAsync();
        return Ok(new { succeeded = true, data = ToDto(user, new[] { role }, profile) });
    }

    [HttpPut("{id}/role")]
    public async Task<IActionResult> SetRole(string id, [FromBody] SetAdminUserRoleRequest request, CancellationToken cancellationToken = default)
    {
        var role = NormalizeRole(request.Role);
        if (role == null) return BadRequest(new { succeeded = false, message = "Role must be Admin or User." });
        var user = await _users.FindByIdAsync(id);
        if (user == null) return NotFound(new { succeeded = false, message = "User not found." });
        if (role != "Admin" && await _users.IsInRoleAsync(user, "Admin"))
        {
            var admins = await _users.GetUsersInRoleAsync("Admin");
            if (admins.Count <= 1) return Conflict(new { succeeded = false, message = "The last administrator cannot be demoted." });
        }

        var currentRoles = await _users.GetRolesAsync(user);
        var removed = await _users.RemoveFromRolesAsync(user, currentRoles);
        if (!removed.Succeeded) return BadRequest(new { succeeded = false, errors = removed.Errors.Select(error => error.Description) });
        var added = await _users.AddToRoleAsync(user, role);
        if (!added.Succeeded)
        {
            await _users.AddToRolesAsync(user, currentRoles);
            return BadRequest(new { succeeded = false, errors = added.Errors.Select(error => error.Description) });
        }
        var profile = await _profileRepository.Query().IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(item => item.UserId == user.Id, cancellationToken);
        return Ok(new { succeeded = true, data = ToDto(user, new[] { role }, profile) });
    }

    [HttpPut("{id}/lock")]
    public async Task<IActionResult> SetLock(string id, [FromBody] SetAdminUserLockRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _users.FindByIdAsync(id);
        if (user == null) return NotFound(new { succeeded = false, message = "User not found." });
        var currentId = _users.GetUserId(User);
        if (request.Locked && string.Equals(currentId, id, StringComparison.Ordinal))
            return Conflict(new { succeeded = false, message = "You cannot lock your own account." });
        if (request.Locked && await _users.IsInRoleAsync(user, "Admin"))
        {
            var admins = await _users.GetUsersInRoleAsync("Admin");
            var availableAdmins = admins.Count(admin => admin.LockoutEnd == null || admin.LockoutEnd <= DateTimeOffset.UtcNow);
            if (availableAdmins <= 1)
                return Conflict(new { succeeded = false, message = "The last available administrator cannot be locked." });
        }

        var enabled = await _users.SetLockoutEnabledAsync(user, true);
        if (!enabled.Succeeded) return BadRequest(new { succeeded = false, errors = enabled.Errors.Select(error => error.Description) });
        var result = await _users.SetLockoutEndDateAsync(user, request.Locked ? DateTimeOffset.UtcNow.AddYears(100) : null);
        if (!result.Succeeded) return BadRequest(new { succeeded = false, errors = result.Errors.Select(error => error.Description) });
        var roles = await _users.GetRolesAsync(user);
        var profile = await _profileRepository.Query().IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(item => item.UserId == user.Id, cancellationToken);
        return Ok(new { succeeded = true, data = ToDto(user, roles, profile) });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken = default)
    {
        var user = await _users.FindByIdAsync(id);
        if (user == null) return NotFound(new { succeeded = false, message = "User not found." });
        var currentId = _users.GetUserId(User);
        if (string.Equals(currentId, id, StringComparison.Ordinal))
            return Conflict(new { succeeded = false, message = "You cannot delete your own account." });
        if (await _users.IsInRoleAsync(user, "Admin"))
        {
            var admins = await _users.GetUsersInRoleAsync("Admin");
            if (admins.Count <= 1) return Conflict(new { succeeded = false, message = "The last administrator cannot be deleted." });
        }

        await _cleanupService.CleanupUserDataAsync(id, cancellationToken);

        var deleted = await _users.DeleteAsync(user);
        return deleted.Succeeded
            ? NoContent()
            : BadRequest(new { succeeded = false, errors = deleted.Errors.Select(error => error.Description) });
    }

    private static string? NormalizeRole(string? role) => role?.Trim().ToLowerInvariant() switch
    {
        "admin" => "Admin",
        "user" or "guest" => "User",
        _ => null
    };

    private static AdminUserDto ToDto(IdentityUser user, IEnumerable<string> roles, MyPortfolio.Domain.Entities.UserProfile? profile) =>
        new(user.Id, user.Email ?? string.Empty, user.UserName ?? string.Empty, roles, user.LockoutEnd,
            profile?.FullName ?? string.Empty, profile?.Title ?? string.Empty, profile?.Location ?? string.Empty,
            profile?.Phone ?? string.Empty, profile?.AvailableForHire ?? false);
}

public sealed record CreateAdminUserRequest(string Email, string Password, string Role = "User", string FullName = "");
public sealed record SetAdminUserRoleRequest(string Role);
public sealed record SetAdminUserLockRequest(bool Locked);
public sealed record AdminUserDto(string Id, string Email, string UserName, IEnumerable<string> Roles, DateTimeOffset? LockoutEnd,
    string FullName, string PortfolioTitle, string Location, string Phone, bool AvailableForHire);
