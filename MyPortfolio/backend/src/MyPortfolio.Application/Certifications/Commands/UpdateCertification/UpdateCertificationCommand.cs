using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Certifications.Models;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.Certifications.Commands.UpdateCertification;

public record UpdateCertificationCommand(
    Guid Id,
    string Name,
    string IssuingOrganization,
    DateTime IssueDate,
    string? CredentialId,
    string? CredentialUrl,
    string? Description,
    int DisplayOrder) : IRequest<Result<CertificationDto>>;

public class UpdateCertificationCommandHandler : IRequestHandler<UpdateCertificationCommand, Result<CertificationDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateCertificationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CertificationDto>> Handle(
        UpdateCertificationCommand request,
        CancellationToken cancellationToken)
    {
        var certification = await _context.Certifications
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (certification == null)
        {
            return Result<CertificationDto>.Failure($"Certification with ID '{request.Id}' was not found.");
        }

        certification.UpdateDetails(
            name: request.Name,
            issuingOrganization: request.IssuingOrganization,
            issueDate: request.IssueDate,
            description: request.Description);

        certification.UpdateCredential(
            credentialId: request.CredentialId,
            credentialUrl: request.CredentialUrl);

        certification.UpdateDisplayOrder(request.DisplayOrder);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<CertificationDto>.Success(
            CertificationDto.FromEntity(certification),
            "Certification updated successfully.");
    }
}
