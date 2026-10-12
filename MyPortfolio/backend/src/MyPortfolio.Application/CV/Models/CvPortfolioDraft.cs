namespace MyPortfolio.Application.CV.Models;

public record CvPortfolioDraft(
    CvPersonalInfoDraft? PersonalInfo,
    List<CvSkillCategoryDraft> Skills,
    List<CvExperienceDraft> Experiences,
    List<CvProjectDraft> Projects,
    List<CvEducationDraft> Education,
    List<CvCertificationDraft> Certifications,
    List<CvSocialLinkDraft> SocialLinks)
{
    public static CvPortfolioDraft CreateEmpty() => new(
        PersonalInfo: null,
        Skills: [],
        Experiences: [],
        Projects: [],
        Education: [],
        Certifications: [],
        SocialLinks: []);
}
