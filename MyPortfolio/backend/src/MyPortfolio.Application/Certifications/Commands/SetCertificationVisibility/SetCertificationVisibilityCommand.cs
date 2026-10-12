using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Certifications.Models;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.Certifications.Commands.SetCertificationVisibility;

public record SetCertificationVisibilityCommand(
    Guid Id,
    bool IsVisible) : IRequest<Result<CertificationDto>>;

public class SetCertificationVisibilityCommandHandler : IRequestHandler<SetCertificationVisibilityCommand, Result<CertificationDto>>
{
    private readonly IApplicationDbContext _context;

    public SetCertificationVisibilityCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CertificationDto>> Handle(
        SetCertificationVisibilityCommand request,
        CancellationToken cancellationToken)
    {
        var certification = await _context.Certifications
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (certification == null)
        {
            return Result<CertificationDto>.Failure($"Certification with ID '{request.Id}' was not found.");
        }

        certification.SetVisibility(request.IsVisible);
        await _context.SaveChangesAsync(cancellationToken);

        var status = request.IsVisible ? "visible" : "hidden";
        return Result<CertificationDto>.Success(
            CertificationDto.FromEntity(certification),
            $"Certification '{certification.Name}' visibility set to {status}.");
    }
}
