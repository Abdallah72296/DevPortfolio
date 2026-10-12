using MediatR;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.Educations.Models;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.Educations.Commands.CreateEducation;

public record CreateEducationCommand(
    string InstitutionName,
    string Degree,
    string? FieldOfStudy,
    string? Description,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsCurrent,
    int DisplayOrder) : IRequest<Result<EducationDto>>;

public class CreateEducationCommandHandler : IRequestHandler<CreateEducationCommand, Result<EducationDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateEducationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<EducationDto>> Handle(
        CreateEducationCommand request,
        CancellationToken cancellationToken)
    {
        var education = new Education(
            institutionName: request.InstitutionName,
            degree: request.Degree,
            startDate: request.StartDate,
            endDate: request.EndDate,
            isCurrent: request.IsCurrent,
            fieldOfStudy: request.FieldOfStudy,
            description: request.Description,
            displayOrder: request.DisplayOrder,
            isVisible: true);

        _context.Educations.Add(education);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<EducationDto>.Success(
            EducationDto.FromEntity(education),
            "Education created successfully.");
    }
}
