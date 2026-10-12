using MediatR;
using MyPortfolio.Application.Authentication.Models;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.Authentication.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponseDto>>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.LoginAsync(request.Email, request.Password, cancellationToken);
    }
}
