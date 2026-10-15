using MediatR;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.Authentication.Commands.ConfirmEmail;

public sealed record ConfirmEmailCommand(string Email, string Token) : IRequest<Result>;

public sealed class ConfirmEmailCommandHandler(IEmailConfirmationService confirmationService)
    : IRequestHandler<ConfirmEmailCommand, Result>
{
    public Task<Result> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken) =>
        confirmationService.ConfirmLinkAsync(request.Email, request.Token, cancellationToken);
}
