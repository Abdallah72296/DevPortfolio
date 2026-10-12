namespace MyPortfolio.API.Models;

public record UpdateSkillCategoryRequest(string Name, int DisplayOrder);

public record SetSkillCategoryVisibilityRequest(bool IsVisible);

public record UpdateSkillRequest(string Name, string? Icon, int DisplayOrder);

public record SetSkillVisibilityRequest(bool IsVisible);

public record MoveSkillRequest(Guid TargetCategoryId);
