using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.Certifications.Commands.DeleteCertification;

public record DeleteCertificationCommand(Guid Id) : IRequest<Result>;

public class DeleteCertificationCommandHandler : IRequestHandler<DeleteCertificationCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteCertificationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        DeleteCertificationCommand request,
        CancellationToken cancellationToken)
    {
        var certification = await _context.Certifications
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (certification == null)
        {
            return Result.Failure($"Certification with ID '{request.Id}' was not found.");
        }

        _context.Certifications.Remove(certification);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success($"Certification '{certification.Name}' deleted successfully.");
    }
}
