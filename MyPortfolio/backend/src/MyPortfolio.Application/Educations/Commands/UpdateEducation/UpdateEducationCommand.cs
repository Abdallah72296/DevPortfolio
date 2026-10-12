using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Educations.Models;

namespace MyPortfolio.Application.Educations.Commands.UpdateEducation;

public record UpdateEducationCommand(
    Guid Id,
    string InstitutionName,
    string Degree,
    string? FieldOfStudy,
    string? Description,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsCurrent,
    int DisplayOrder) : IRequest<Result<EducationDto>>;

public class UpdateEducationCommandHandler : IRequestHandler<UpdateEducationCommand, Result<EducationDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateEducationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<EducationDto>> Handle(
        UpdateEducationCommand request,
        CancellationToken cancellationToken)
    {
        var education = await _context.Educations
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (education == null)
        {
            return Result<EducationDto>.Failure($"Education with ID '{request.Id}' was not found.");
        }

        education.UpdateDetails(
            institutionName: request.InstitutionName,
            degree: request.Degree,
            fieldOfStudy: request.FieldOfStudy,
            description: request.Description);

        education.UpdatePeriod(
            startDate: request.StartDate,
            endDate: request.EndDate,
            isCurrent: request.IsCurrent);

        education.UpdateDisplayOrder(request.DisplayOrder);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<EducationDto>.Success(
            EducationDto.FromEntity(education),
            "Education updated successfully.");
    }
}
