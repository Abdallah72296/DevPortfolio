using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.AdminUsers.Models;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Domain.Entities;
using MyPortfolio.Infrastructure.Data;

namespace MyPortfolio.Infrastructure.Authentication;

public sealed class AdminUserManagementService : IAdminUserManagementService
{
    private readonly UserManager<IdentityUser> _users;
    private readonly RoleManager<IdentityRole> _roles;
    private readonly ApplicationDbContext _db;
    private readonly IUserDataCleanupService _cleanup;

    public AdminUserManagementService(
        UserManager<IdentityUser> users,
        RoleManager<IdentityRole> roles,
        ApplicationDbContext db,
        IUserDataCleanupService cleanup)
    {
        _users = users;
        _roles = roles;
        _db = db;
        _cleanup = cleanup;
    }

    public async Task<Result<AdminUserDirectoryPage>> GetDirectoryAsync(
        int pageNumber, int pageSize, string? search, string? role, string? status, CancellationToken cancellationToken)
    {
        var query = _users.Users.AsNoTracking();
        var term = search?.Trim();
        if (!string.IsNullOrWhiteSpace(term))
        {
            var matchingProfileIds = _db.UserProfiles.IgnoreQueryFilters().AsNoTracking()
                .Where(profile => profile.FullName.Contains(term) || profile.Title.Contains(term) || profile.Location.Contains(term))
                .Select(profile => profile.UserId);
            query = query.Where(user => (user.Email != null && user.Email.Contains(term)) ||
                                        (user.UserName != null && user.UserName.Contains(term)) ||
                                        matchingProfileIds.Contains(user.Id));
        }

        if (role is "Admin" or "User")
        {
            var normalizedRole = _roles.NormalizeKey(role);
            var userIdsInRole = from userRole in _db.UserRoles
                                join identityRole in _db.Roles on userRole.RoleId equals identityRole.Id
                                where identityRole.NormalizedName == normalizedRole
                                select userRole.UserId;
            query = query.Where(user => userIdsInRole.Contains(user.Id));
        }

        var now = DateTimeOffset.UtcNow;
        if (status == "Locked") query = query.Where(user => user.LockoutEnd.HasValue && user.LockoutEnd > now);
        else if (status == "Active") query = query.Where(user => !user.LockoutEnd.HasValue || user.LockoutEnd <= now);

        var totalCount = await query.CountAsync(cancellationToken);
        var pageUsers = await query.OrderBy(user => user.Email).ThenBy(user => user.Id)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(user => new AdminUserDto(user.Id, user.Email ?? string.Empty, user.UserName ?? string.Empty,
                Array.Empty<string>(), user.LockoutEnd, string.Empty, string.Empty, string.Empty, string.Empty, false))
            .ToListAsync(cancellationToken);

        var userIds = pageUsers.Select(user => user.Id).ToArray();
        if (userIds.Length > 0)
        {
            var rolesByUser = await (from userRole in _db.UserRoles
                                     join identityRole in _db.Roles on userRole.RoleId equals identityRole.Id
                                     where userIds.Contains(userRole.UserId)
                                     select new { userRole.UserId, identityRole.Name })
                .ToListAsync(cancellationToken);
            var profiles = await _db.UserProfiles.IgnoreQueryFilters().AsNoTracking()
                .Where(profile => userIds.Contains(profile.UserId))
                .ToDictionaryAsync(profile => profile.UserId, cancellationToken);
            var rolesLookup = rolesByUser.ToLookup(item => item.UserId, item => item.Name ?? string.Empty);

            pageUsers = pageUsers.Select(user =>
            {
                profiles.TryGetValue(user.Id, out var profile);
                return user with
                {
                    Roles = rolesLookup[user.Id].ToArray(),
                    FullName = profile?.FullName ?? string.Empty,
                    PortfolioTitle = profile?.Title ?? string.Empty,
                    Location = profile?.Location ?? string.Empty,
                    Phone = profile?.Phone ?? string.Empty,
                    AvailableForHire = profile?.AvailableForHire ?? false
                };
            }).ToList();
        }

        var allUsers = _users.Users.AsNoTracking();
        var totalUsers = await allUsers.CountAsync(cancellationToken);
        var lockedUsers = await allUsers.CountAsync(user => user.LockoutEnd.HasValue && user.LockoutEnd > now, cancellationToken);
        var adminRole = await _roles.FindByNameAsync("Admin");
        var adminUsers = adminRole is null ? 0 : await _db.UserRoles.CountAsync(userRole => userRole.RoleId == adminRole.Id, cancellationToken);
        var availableForHire = await _db.UserProfiles.IgnoreQueryFilters().CountAsync(profile => profile.AvailableForHire, cancellationToken);
        var summary = new AdminUserDirectorySummary(totalUsers, totalUsers - lockedUsers, lockedUsers,
            adminUsers, Math.Max(0, totalUsers - adminUsers), availableForHire);
        var page = new AdminUserDirectoryPage(pageUsers, pageNumber, pageSize, totalCount,
            (int)Math.Ceiling(totalCount / (double)pageSize), summary);
        return Result<AdminUserDirectoryPage>.Success(page);
    }

    public async Task<Result<AdminUserDto>> CreateAsync(CreateAdminUserRequest request, CancellationToken cancellationToken)
    {
        var role = NormalizeRole(request.Role);
        if (role is null) return Result<AdminUserDto>.Failure("Role must be Admin or User.");
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return Result<AdminUserDto>.Failure("Email and password are required.");

        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            var email = request.Email.Trim();
            var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = false };
            var created = await _users.CreateAsync(user, request.Password);
            if (!created.Succeeded)
                return Result<AdminUserDto>.Failure(created.Errors.Select(error => error.Description).ToList(), "Account could not be created.");

            var assigned = await _users.AddToRoleAsync(user, role);
            if (!assigned.Succeeded)
                return Result<AdminUserDto>.Failure(assigned.Errors.Select(error => error.Description).ToList(), "Role could not be assigned.");

            var fullName = string.IsNullOrWhiteSpace(request.FullName) ? email.Split('@')[0] : request.FullName.Trim();
            var profile = new UserProfile(user.Id, fullName, "Developer", "Developer", "Portfolio",
                string.Empty, user.Email!, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, "", "[]", false);
            profile.AssignToUser(user.Id);
            await _db.UserProfiles.AddAsync(profile, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return Result<AdminUserDto>.Success(ToDto(user, [role], profile));
        });
    }

    public async Task<Result<AdminUserDto>> SetRoleAsync(string id, string roleInput, CancellationToken cancellationToken)
    {
        var role = NormalizeRole(roleInput);
        if (role is null) return Result<AdminUserDto>.Failure("Role must be Admin or User.");
        var user = await _users.FindByIdAsync(id);
        if (user is null) return Result<AdminUserDto>.Failure("User not found.");
        var currentRoles = await _users.GetRolesAsync(user);
        if (role != "Admin" && currentRoles.Contains("Admin") &&
            (await _users.GetUsersInRoleAsync("Admin")).Count <= 1)
            return Result<AdminUserDto>.Failure("The last administrator cannot be demoted.");

        var strategy = _db.Database.CreateExecutionStrategy();
        var updateResult = await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            var removed = await _users.RemoveFromRolesAsync(user, currentRoles);
            if (!removed.Succeeded)
                return Result.Failure(removed.Errors.Select(error => error.Description).ToList(), "Role could not be changed.");
            var added = await _users.AddToRoleAsync(user, role);
            if (!added.Succeeded)
                return Result.Failure(added.Errors.Select(error => error.Description).ToList(), "Role could not be changed.");

            await transaction.CommitAsync(cancellationToken);
            return Result.Success();
        });
        if (!updateResult.Succeeded)
            return Result<AdminUserDto>.Failure(updateResult.Errors, updateResult.Message ?? "Role could not be changed.");
        var profile = await _db.UserProfiles.IgnoreQueryFilters().AsNoTracking()
            .FirstOrDefaultAsync(item => item.UserId == user.Id, cancellationToken);
        return Result<AdminUserDto>.Success(ToDto(user, [role], profile));
    }

    public async Task<Result<AdminUserDto>> SetLockAsync(string id, bool locked, string? currentAdminId, CancellationToken cancellationToken)
    {
        var user = await _users.FindByIdAsync(id);
        if (user is null) return Result<AdminUserDto>.Failure("User not found.");
        if (locked && string.Equals(currentAdminId, id, StringComparison.Ordinal))
            return Result<AdminUserDto>.Failure("You cannot lock your own account.");
        if (locked && await _users.IsInRoleAsync(user, "Admin"))
        {
            var admins = await _users.GetUsersInRoleAsync("Admin");
            var availableAdmins = admins.Count(admin => admin.LockoutEnd is null || admin.LockoutEnd <= DateTimeOffset.UtcNow);
            if (availableAdmins <= 1) return Result<AdminUserDto>.Failure("The last available administrator cannot be locked.");
        }

        var enabled = await _users.SetLockoutEnabledAsync(user, true);
        if (!enabled.Succeeded) return Result<AdminUserDto>.Failure(enabled.Errors.Select(error => error.Description).ToList());
        var result = await _users.SetLockoutEndDateAsync(user, locked ? DateTimeOffset.UtcNow.AddYears(100) : null);
        if (!result.Succeeded) return Result<AdminUserDto>.Failure(result.Errors.Select(error => error.Description).ToList());
        var roles = await _users.GetRolesAsync(user);
        var profile = await _db.UserProfiles.IgnoreQueryFilters().AsNoTracking()
            .FirstOrDefaultAsync(item => item.UserId == user.Id, cancellationToken);
        return Result<AdminUserDto>.Success(ToDto(user, roles, profile));
    }

    public async Task<Result> DeleteAsync(string id, string? currentAdminId, CancellationToken cancellationToken)
    {
        var user = await _users.FindByIdAsync(id);
        if (user is null) return Result.Failure("User not found.");
        if (string.Equals(currentAdminId, id, StringComparison.Ordinal))
            return Result.Failure("You cannot delete your own account.");
        if (await _users.IsInRoleAsync(user, "Admin") && (await _users.GetUsersInRoleAsync("Admin")).Count <= 1)
            return Result.Failure("The last administrator cannot be deleted.");

        var strategy = _db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            await _cleanup.CleanupUserDataAsync(id, cancellationToken);
            var deleted = await _users.DeleteAsync(user);
            if (!deleted.Succeeded)
                return Result.Failure(deleted.Errors.Select(error => error.Description).ToList(), "Account could not be deleted.");
            await transaction.CommitAsync(cancellationToken);
            return Result.Success("User and associated data deleted.");
        });
    }

    private static string? NormalizeRole(string? role) => role?.Trim().ToLowerInvariant() switch
    {
        "admin" => "Admin",
        "user" or "guest" => "User",
        _ => null
    };

    private static AdminUserDto ToDto(IdentityUser user, IEnumerable<string> roles, UserProfile? profile) =>
        new(user.Id, user.Email ?? string.Empty, user.UserName ?? string.Empty, roles, user.LockoutEnd,
            profile?.FullName ?? string.Empty, profile?.Title ?? string.Empty, profile?.Location ?? string.Empty,
            profile?.Phone ?? string.Empty, profile?.AvailableForHire ?? false);
}
