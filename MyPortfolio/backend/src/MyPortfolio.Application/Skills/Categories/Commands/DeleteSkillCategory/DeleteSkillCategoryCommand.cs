using MediatR;
using Microsoft.EntityFrameworkCore;
using MyPortfolio.Application.Common.Interfaces;
using MyPortfolio.Application.Common.Models;

namespace MyPortfolio.Application.Skills.Categories.Commands.DeleteSkillCategory;

public record DeleteSkillCategoryCommand(Guid Id) : IRequest<Result>;

public class DeleteSkillCategoryCommandHandler : IRequestHandler<DeleteSkillCategoryCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteSkillCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        DeleteSkillCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var category = await _context.SkillCategories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
        {
            return Result.Failure($"Skill category with ID '{request.Id}' was not found.");
        }

        // Cascade delete on foreign key removes associated skills automatically in database
        _context.SkillCategories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success($"Skill category '{category.Name}' and its associated skills were deleted successfully.");
    }
}
