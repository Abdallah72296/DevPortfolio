using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Educations.Models;

namespace MyPortfolio.Application.Educations.Queries.GetEducations;

public record GetEducationsQuery : IRequest<Result<List<EducationDto>>>;

public class GetEducationsQueryHandler : IRequestHandler<GetEducationsQuery, Result<List<EducationDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetEducationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<EducationDto>>> Handle(
        GetEducationsQuery request,
        CancellationToken cancellationToken)
    {
        var educations = await _context.Educations
            .AsNoTracking()
            .OrderBy(e => e.DisplayOrder)
            .ToListAsync(cancellationToken);

        var dtos = educations.Select(EducationDto.FromEntity).ToList();

        return Result<List<EducationDto>>.Success(dtos);
    }
}
