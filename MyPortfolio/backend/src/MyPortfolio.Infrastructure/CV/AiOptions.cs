namespace MyPortfolio.Infrastructure.CV;

public class AiOptions
{
    public const string SectionName = "AI";

    public string Provider { get; set; } = "Gemini";
    public string Model { get; set; } = "gemini-2.5-flash";
    public string? ApiKey { get; set; }
}
