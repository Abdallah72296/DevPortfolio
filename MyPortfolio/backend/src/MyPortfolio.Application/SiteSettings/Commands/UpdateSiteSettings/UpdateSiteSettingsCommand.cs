using System.Text.RegularExpressions;
using MediatR;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.SiteSettings.Models;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.SiteSettings.Commands.UpdateSiteSettings;

public record UpdateSiteSettingsCommand(
    string ThemeKey,
    string? CustomDomain,
    string? SeoTitle,
    string? SeoDescription,
    string? SeoKeywords) : IRequest<Result<PortfolioSiteSettingsDto>>;

public class UpdateSiteSettingsCommandHandler : IRequestHandler<UpdateSiteSettingsCommand, Result<PortfolioSiteSettingsDto>>
{
    private readonly IGenericRepository<PortfolioSiteSettings> _repository;

    public UpdateSiteSettingsCommandHandler(IGenericRepository<PortfolioSiteSettings> repository)
    {
        _repository = repository;
    }

    public async Task<Result<PortfolioSiteSettingsDto>> Handle(UpdateSiteSettingsCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.ThemeKey) || request.ThemeKey.Length > 80)
            return Result<PortfolioSiteSettingsDto>.Failure("A valid theme key is required.");

        if (!string.IsNullOrWhiteSpace(request.CustomDomain) &&
            !Regex.IsMatch(request.CustomDomain, @"^(?=.{1,253}$)([a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,63}$"))
            return Result<PortfolioSiteSettingsDto>.Failure("Custom domain must be a valid hostname without scheme or path.");

        if (request.SeoTitle?.Length > 200 || request.SeoDescription?.Length > 500 || request.SeoKeywords?.Length > 1000)
            return Result<PortfolioSiteSettingsDto>.Failure("SEO field length exceeds the supported limit.");

        var settings = await _repository.FirstOrDefaultAsync(s => true, cancellationToken);
        if (settings == null)
        {
            settings = new PortfolioSiteSettings();
            await _repository.AddAsync(settings, cancellationToken);
        }

        settings.Update(
            request.ThemeKey,
            request.CustomDomain ?? string.Empty,
            request.SeoTitle ?? string.Empty,
            request.SeoDescription ?? string.Empty,
            request.SeoKeywords ?? string.Empty);
        settings.MarkPublished();

        _repository.Update(settings);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result<PortfolioSiteSettingsDto>.Success(PortfolioSiteSettingsDto.From(settings));
    }
}
