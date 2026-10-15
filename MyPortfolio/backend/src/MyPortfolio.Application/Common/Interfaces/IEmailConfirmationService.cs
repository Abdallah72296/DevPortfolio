using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Authentication.Models;

namespace MyPortfolio.Application.Common.Interfaces;

public interface IEmailConfirmationService
{
    Task<Result<RegistrationResponseDto>> StartRegistrationAsync(string fullName, string username, string email, string password, CancellationToken cancellationToken = default);
    Task<bool> SendOtpAsync(string email, CancellationToken cancellationToken = default);
    Task<Result> ConfirmLinkAsync(string email, string token, CancellationToken cancellationToken = default);
    Task<Result> ConfirmOtpAsync(string email, string code, CancellationToken cancellationToken = default);
    Task<Result> ResendOtpAsync(string email, CancellationToken cancellationToken = default);
}
