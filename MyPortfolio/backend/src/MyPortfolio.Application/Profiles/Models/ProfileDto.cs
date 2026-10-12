namespace MyPortfolio.Application.Profiles.Models;

public class ProfileDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string HeadlineRole { get; set; } = string.Empty;
    public string Badge { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string CardCaption { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string YearsOfExperience { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string LongBio { get; set; } = string.Empty;
    public List<string> Paragraphs { get; set; } = new();
    public string AvatarUrl { get; set; } = string.Empty;
    public string CvUrl { get; set; } = string.Empty;
    public bool AvailableForHire { get; set; } = true;
}
