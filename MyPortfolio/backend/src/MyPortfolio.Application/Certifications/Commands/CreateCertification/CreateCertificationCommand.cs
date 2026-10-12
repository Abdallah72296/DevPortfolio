using MediatR;
using MyPortfolio.Application.Certifications.Models;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Domain.Entities;

namespace MyPortfolio.Application.Certifications.Commands.CreateCertification;

public record CreateCertificationCommand(
    string Name,
    string IssuingOrganization,
    DateTime IssueDate,
    string? CredentialId,
    string? CredentialUrl,
    string? Description,
    int DisplayOrder) : IRequest<Result<CertificationDto>>;

public class CreateCertificationCommandHandler : IRequestHandler<CreateCertificationCommand, Result<CertificationDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateCertificationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CertificationDto>> Handle(
        CreateCertificationCommand request,
        CancellationToken cancellationToken)
    {
        var certification = new Certification(
            name: request.Name,
            issuingOrganization: request.IssuingOrganization,
            issueDate: request.IssueDate,
            credentialId: request.CredentialId,
            credentialUrl: request.CredentialUrl,
            description: request.Description,
            displayOrder: request.DisplayOrder,
            isVisible: true);

        _context.Certifications.Add(certification);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<CertificationDto>.Success(
            CertificationDto.FromEntity(certification),
            "Certification created successfully.");
    }
}
