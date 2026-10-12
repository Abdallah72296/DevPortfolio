namespace MyPortfolio.Application.Portfolio.Models;

public record PublicSkillDto(
    Guid Id,
    string Name,
    string? Icon,
    int DisplayOrder);
