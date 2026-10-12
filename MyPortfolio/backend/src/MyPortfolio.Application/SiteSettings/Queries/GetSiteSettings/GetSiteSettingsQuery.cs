using MediatR;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.SiteSettings.Models;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.SiteSettings.Queries.GetSiteSettings;

public record GetSiteSettingsQuery(bool CreateIfNotExists = true) : IRequest<Result<PortfolioSiteSettingsDto?>>;

public class GetSiteSettingsQueryHandler : IRequestHandler<GetSiteSettingsQuery, Result<PortfolioSiteSettingsDto?>>
{
    private readonly IGenericRepository<PortfolioSiteSettings> _repository;

    public GetSiteSettingsQueryHandler(IGenericRepository<PortfolioSiteSettings> repository)
    {
        _repository = repository;
    }

    public async Task<Result<PortfolioSiteSettingsDto?>> Handle(GetSiteSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _repository.FirstOrDefaultAsync(s => true, cancellationToken);
        if (settings == null)
        {
            if (!request.CreateIfNotExists)
            {
                return Result<PortfolioSiteSettingsDto?>.Success(null);
            }

            settings = new PortfolioSiteSettings();
            await _repository.AddAsync(settings, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
        }

        return Result<PortfolioSiteSettingsDto?>.Success(PortfolioSiteSettingsDto.From(settings));
    }
}
