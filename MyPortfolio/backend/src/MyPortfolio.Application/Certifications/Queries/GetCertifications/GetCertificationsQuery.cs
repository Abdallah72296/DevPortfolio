using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Certifications.Models;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.Certifications.Queries.GetCertifications;

public record GetCertificationsQuery : IRequest<Result<List<CertificationDto>>>;

public class GetCertificationsQueryHandler : IRequestHandler<GetCertificationsQuery, Result<List<CertificationDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetCertificationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<CertificationDto>>> Handle(
        GetCertificationsQuery request,
        CancellationToken cancellationToken)
    {
        var certifications = await _context.Certifications
            .AsNoTracking()
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);

        var dtos = certifications.Select(CertificationDto.FromEntity).ToList();

        return Result<List<CertificationDto>>.Success(dtos);
    }
}
