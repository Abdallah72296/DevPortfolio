using MyPortfolio.Application.AdminUsers.Models;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.Common.Interfaces;

public interface IAdminUserManagementService
{
    Task<Result<AdminUserDirectoryPage>> GetDirectoryAsync(
        int pageNumber, int pageSize, string? search, string? role, string? status, CancellationToken cancellationToken);
    Task<Result<AdminUserDto>> CreateAsync(CreateAdminUserRequest request, CancellationToken cancellationToken);
    Task<Result<AdminUserDto>> SetRoleAsync(string id, string role, CancellationToken cancellationToken);
    Task<Result<AdminUserDto>> SetLockAsync(string id, bool locked, string? currentAdminId, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(string id, string? currentAdminId, CancellationToken cancellationToken);
}
