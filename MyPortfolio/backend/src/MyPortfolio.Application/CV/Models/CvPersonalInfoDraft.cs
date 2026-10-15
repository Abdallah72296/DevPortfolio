namespace MyPortfolio.Application.CV.Models;

public record CvPersonalInfoDraft(
    string? Name,
    string? JobTitle,
    string? About,
    string? Email = null,
    string? Phone = null,
    string? Location = null);
