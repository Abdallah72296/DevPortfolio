using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.Educations.Commands.DeleteEducation;

public record DeleteEducationCommand(Guid Id) : IRequest<Result>;

public class DeleteEducationCommandHandler : IRequestHandler<DeleteEducationCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteEducationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        DeleteEducationCommand request,
        CancellationToken cancellationToken)
    {
        var education = await _context.Educations
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (education == null)
        {
            return Result.Failure($"Education with ID '{request.Id}' was not found.");
        }

        _context.Educations.Remove(education);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success($"Education '{education.Degree}' deleted successfully.");
    }
}
