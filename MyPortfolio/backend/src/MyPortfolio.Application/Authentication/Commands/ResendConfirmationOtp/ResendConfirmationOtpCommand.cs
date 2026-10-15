using MediatR;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.Authentication.Commands.ResendConfirmationOtp;

public sealed record ResendConfirmationOtpCommand(string Email) : IRequest<Result>;

public sealed class ResendConfirmationOtpCommandHandler(IEmailConfirmationService confirmationService)
    : IRequestHandler<ResendConfirmationOtpCommand, Result>
{
    public Task<Result> Handle(ResendConfirmationOtpCommand request, CancellationToken cancellationToken) =>
        confirmationService.ResendOtpAsync(request.Email, cancellationToken);
}
