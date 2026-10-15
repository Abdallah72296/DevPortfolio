namespace MyPortfolio.Application.AdminUsers.Models;

public sealed record AdminUserDto(
    string Id,
    string Email,
    string UserName,
    IEnumerable<string> Roles,
    DateTimeOffset? LockoutEnd,
    string FullName,
    string PortfolioTitle,
    string Location,
    string Phone,
    bool AvailableForHire);

public sealed record AdminUserDirectorySummary(
    int TotalUsers,
    int ActiveUsers,
    int LockedUsers,
    int AdminUsers,
    int StandardUsers,
    int AvailableForHireUsers);

public sealed record AdminUserDirectoryPage(
    IReadOnlyList<AdminUserDto> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages,
    AdminUserDirectorySummary Summary);
public sealed record CreateAdminUserRequest(string Email, string Password, string Role = "User", string FullName = "");
public sealed record SetAdminUserRoleRequest(string Role);
public sealed record SetAdminUserLockRequest(bool Locked);
