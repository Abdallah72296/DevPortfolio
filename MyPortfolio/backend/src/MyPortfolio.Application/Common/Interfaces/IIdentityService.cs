using MyPortfolio.Application.Authentication.Models;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.Common.Interfaces;

/// <summary>
/// Abstraction for identity and authentication operations.
/// Shields the Application layer from direct dependency on ASP.NET Core Identity and JWT generation details.
/// </summary>
public interface IIdentityService
{
    Task<Result<AuthResponseDto>> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<Result<RegistrationResponseDto>> RegisterAsync(string fullName, string username, string email, string password, CancellationToken cancellationToken = default);
}
