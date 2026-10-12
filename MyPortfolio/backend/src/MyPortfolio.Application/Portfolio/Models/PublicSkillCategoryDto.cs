namespace MyPortfolio.Application.Portfolio.Models;

public record PublicSkillCategoryDto(
    Guid CategoryId,
    string Name,
    int DisplayOrder,
    List<PublicSkillDto> Skills);
