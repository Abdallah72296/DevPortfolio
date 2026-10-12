using MediatR;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.CV.Models;

namespace MyPortfolio.Application.CV.Commands.ParseCv;

public record ParseCvCommand(
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSize) : IRequest<Result<CvPortfolioDraft>>;
