using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace MyPortfolio.Infrastructure.Authentication;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(IdentityUser user, IList<string> roles);
}
