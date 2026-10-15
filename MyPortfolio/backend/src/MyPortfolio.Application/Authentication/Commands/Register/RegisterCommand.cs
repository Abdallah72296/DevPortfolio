using MediatR;
using MyPortfolio.Application.Authentication.Models;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.Authentication.Commands.Register;

public sealed record RegisterCommand(string FullName, string Username, string Email, string Password)
    : IRequest<Result<RegistrationResponseDto>>;

public sealed class RegisterCommandHandler(IIdentityService identityService)
    : IRequestHandler<RegisterCommand, Result<RegistrationResponseDto>>
{
    public Task<Result<RegistrationResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken) =>
        identityService.RegisterAsync(request.FullName, request.Username, request.Email, request.Password, cancellationToken);
}
