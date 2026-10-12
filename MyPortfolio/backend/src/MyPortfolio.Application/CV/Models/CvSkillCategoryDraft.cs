namespace MyPortfolio.Application.CV.Models;

public record CvSkillCategoryDraft(
    string Category,
    List<CvSkillDraft> Skills);
