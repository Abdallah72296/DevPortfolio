using MediatR;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.Authentication.Commands.ConfirmEmailOtp;

public sealed record ConfirmEmailOtpCommand(string Email, string Code) : IRequest<Result>;

public sealed class ConfirmEmailOtpCommandHandler(IEmailConfirmationService confirmationService)
    : IRequestHandler<ConfirmEmailOtpCommand, Result>
{
    public Task<Result> Handle(ConfirmEmailOtpCommand request, CancellationToken cancellationToken) =>
        confirmationService.ConfirmOtpAsync(request.Email, request.Code, cancellationToken);
}
