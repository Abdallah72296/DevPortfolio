using MediatR;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.SiteSettings.Commands.PublishSiteSettings;

public record PublishSiteSettingsResult(bool IsPublished, DateTime? PublishedAt, bool DeploymentPerformed = false);

public record PublishSiteSettingsCommand() : IRequest<Result<PublishSiteSettingsResult>>;

public class PublishSiteSettingsCommandHandler : IRequestHandler<PublishSiteSettingsCommand, Result<PublishSiteSettingsResult>>
{
    private readonly IGenericRepository<PortfolioSiteSettings> _repository;

    public PublishSiteSettingsCommandHandler(IGenericRepository<PortfolioSiteSettings> repository)
    {
        _repository = repository;
    }

    public async Task<Result<PublishSiteSettingsResult>> Handle(PublishSiteSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await _repository.FirstOrDefaultAsync(s => true, cancellationToken);
        if (settings == null)
        {
            settings = new PortfolioSiteSettings();
            await _repository.AddAsync(settings, cancellationToken);
        }

        settings.MarkPublished();
        _repository.Update(settings);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result<PublishSiteSettingsResult>.Success(
            new PublishSiteSettingsResult(settings.IsPublished, settings.PublishedAt, false),
            "Publication state recorded. Hosting deployment is not configured.");
    }
}
