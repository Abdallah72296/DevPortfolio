using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.CV.Models;

namespace MyPortfolio.Application.CV.Interfaces;

public interface ICvParser
{
    Task<Result<CvPortfolioDraft>> ParseCvAsync(
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default);
}
