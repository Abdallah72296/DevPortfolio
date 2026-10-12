using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Educations.Models;

namespace MyPortfolio.Application.Educations.Commands.SetEducationVisibility;

public record SetEducationVisibilityCommand(
    Guid Id,
    bool IsVisible) : IRequest<Result<EducationDto>>;

public class SetEducationVisibilityCommandHandler : IRequestHandler<SetEducationVisibilityCommand, Result<EducationDto>>
{
    private readonly IApplicationDbContext _context;

    public SetEducationVisibilityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<EducationDto>> Handle(
        SetEducationVisibilityCommand request,
        CancellationToken cancellationToken)
    {
        var education = await _context.Educations
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (education == null)
        {
            return Result<EducationDto>.Failure($"Education with ID '{request.Id}' was not found.");
        }

        education.SetVisibility(request.IsVisible);
        await _context.SaveChangesAsync(cancellationToken);

        var status = request.IsVisible ? "visible" : "hidden";
        return Result<EducationDto>.Success(
            EducationDto.FromEntity(education),
            $"Education '{education.Degree}' visibility set to {status}.");
    }
}
