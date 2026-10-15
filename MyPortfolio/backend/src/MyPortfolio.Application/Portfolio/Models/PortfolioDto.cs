namespace MyPortfolio.Application.Portfolio.Models;

public record PublicUserProfileDto(
    string FullName,
    string Title,
    string HeadlineRole,
    string Badge,
    string Location,
    string CardCaption,
    string Email,
    string Phone,
    string Summary,
    string LongBio,
    string AvatarUrl,
    string CvUrl,
    string YearsOfExperience,
    List<string> Paragraphs,
    bool AvailableForHire);

public record PortfolioDto(
    List<PublicPortfolioSectionDto> Sections,
    List<PublicSkillCategoryDto> Skills,
    List<PublicExperienceDto> Experiences,
    List<PublicProjectDto> Projects,
    List<PublicEducationDto> Education,
    List<PublicCertificationDto> Certifications,
    List<PublicSocialLinkDto> SocialLinks,
    PublicUserProfileDto? Profile = null,
    string ThemeKey = "obsidian-dark",
    string? SeoTitle = null,
    string? SeoDescription = null,
    string? SeoKeywords = null);
