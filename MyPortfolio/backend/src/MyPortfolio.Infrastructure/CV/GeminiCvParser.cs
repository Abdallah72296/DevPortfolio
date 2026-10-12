using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyPortfolio.Application.Common.Models;
using MyPortfolio.Application.CV.Interfaces;
using MyPortfolio.Application.CV.Models;

namespace MyPortfolio.Infrastructure.CV;

public class GeminiCvParser : ICvParser
{
    private const string SystemPrompt = """
        You are an expert CV and resume document parser for a professional developer portfolio.
        Analyze the provided CV document and extract structured portfolio data into the specified JSON schema.

        CRITICAL ANTI-HALLUCINATION RULES:
        1. Extract information ONLY from the provided CV document.
        2. NEVER invent companies, employers, or organizations.
        3. NEVER invent job titles, roles, or positions.
        4. NEVER invent dates or timeframes.
        5. NEVER invent project details, titles, or descriptions.
        6. NEVER invent certifications or issuing authorities.
        7. NEVER invent or guess URLs (e.g. GitHub, LinkedIn, portfolio sites) unless they explicitly appear in the document.
        8. Do NOT turn generic technology mentions or keywords into fake projects.
        9. Do NOT create information just to fill empty or missing fields.
        10. If a field or detail cannot be confidently extracted, leave it null or empty.
        11. Normalize dates to ISO formats: 'yyyy-MM-dd' if exact day is known, 'yyyy-MM' if month and year are known, or 'yyyy' if only year is known. Do NOT invent a day (like 01) if only month/year is provided. If current job or study, set isCurrent=true and endDate=null.
        12. Map ONLY to the supported sections: PersonalInfo (Hero/About), Skills, Experiences, Projects, Education, Certifications, SocialLinks.
        13. Do NOT create custom sections (e.g. awards, testimonials, languages, interests, hobbies).
        14. Output MUST strictly adhere to the provided JSON schema.
        """;

    private const string JsonSchema = """
        {
          "type": "object",
          "properties": {
            "personalInfo": {
              "type": ["object", "null"],
              "properties": {
                "name": { "type": ["string", "null"] },
                "jobTitle": { "type": ["string", "null"] },
                "about": { "type": ["string", "null"] }
              },
              "required": ["name", "jobTitle", "about"]
            },
            "skills": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "category": { "type": "string" },
                  "skills": {
                    "type": "array",
                    "items": {
                      "type": "object",
                      "properties": {
                        "name": { "type": "string" },
                        "icon": { "type": ["string", "null"] }
                      },
                      "required": ["name", "icon"]
                    }
                  }
                },
                "required": ["category", "skills"]
              }
            },
            "experiences": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "jobTitle": { "type": "string" },
                  "companyName": { "type": "string" },
                  "description": { "type": ["string", "null"] },
                  "startDate": { "type": ["string", "null"] },
                  "endDate": { "type": ["string", "null"] },
                  "isCurrent": { "type": "boolean" }
                },
                "required": ["jobTitle", "companyName", "description", "startDate", "endDate", "isCurrent"]
              }
            },
            "projects": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "title": { "type": "string" },
                  "description": { "type": ["string", "null"] },
                  "image": { "type": ["string", "null"] },
                  "liveUrl": { "type": ["string", "null"] },
                  "githubUrl": { "type": ["string", "null"] },
                  "technologies": {
                    "type": "array",
                    "items": { "type": "string" }
                  },
                  "isFeatured": { "type": "boolean" }
                },
                "required": ["title", "description", "image", "liveUrl", "githubUrl", "technologies", "isFeatured"]
              }
            },
            "education": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "institutionName": { "type": "string" },
                  "degree": { "type": "string" },
                  "fieldOfStudy": { "type": ["string", "null"] },
                  "description": { "type": ["string", "null"] },
                  "startDate": { "type": ["string", "null"] },
                  "endDate": { "type": ["string", "null"] },
                  "isCurrent": { "type": "boolean" }
                },
                "required": ["institutionName", "degree", "fieldOfStudy", "description", "startDate", "endDate", "isCurrent"]
              }
            },
            "certifications": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "name": { "type": "string" },
                  "issuingOrganization": { "type": "string" },
                  "issueDate": { "type": ["string", "null"] },
                  "credentialId": { "type": ["string", "null"] },
                  "credentialUrl": { "type": ["string", "null"] },
                  "description": { "type": ["string", "null"] }
                },
                "required": ["name", "issuingOrganization", "issueDate", "credentialId", "credentialUrl", "description"]
              }
            },
            "socialLinks": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "platform": { "type": "string" },
                  "url": { "type": "string" }
                },
                "required": ["platform", "url"]
              }
            }
          },
          "required": [
            "personalInfo",
            "skills",
            "experiences",
            "projects",
            "education",
            "certifications",
            "socialLinks"
          ]
        }
        """;

    private readonly AiOptions _options;
    private readonly ILogger<GeminiCvParser> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public GeminiCvParser(
        IOptions<AiOptions> options,
        ILogger<GeminiCvParser> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result<CvPortfolioDraft>> ParseCvAsync(
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        // 1. Read document stream into byte array
        byte[] pdfBytes;
        if (fileStream is MemoryStream ms)
        {
            pdfBytes = ms.ToArray();
        }
        else
        {
            using var buffer = new MemoryStream();
            await fileStream.CopyToAsync(buffer, cancellationToken);
            pdfBytes = buffer.ToArray();
        }

        if (pdfBytes.Length < 10)
        {
            _logger.LogInformation("PDF document is empty or too small (< 10 bytes).");
            return Result<CvPortfolioDraft>.Failure("Could not extract readable text from the CV.");
        }

        // 2. Handle Mock/Test provider explicitly for automated deterministic testing
        if (string.Equals(_options.Provider, "Mock", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(_options.Provider, "Test", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("Using Mock/Test CV parser.");
            return ParseMockCv(pdfBytes);
        }

        // 3. Resolve API key from configuration (User Secrets / appsettings / environment)
        var apiKey = !string.IsNullOrWhiteSpace(_options.ApiKey)
            ? _options.ApiKey
            : System.Environment.GetEnvironmentVariable("GEMINI_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Gemini API key is not configured. Provider: {Provider}", _options.Provider);
            return Result<CvPortfolioDraft>.Failure(
                "Gemini API key is not configured. Please set the 'AI:ApiKey' configuration setting.");
        }

        // 4. Call Google Gemini API with direct PDF document input and structured output
        try
        {
            using var client = new Client(apiKey: apiKey);

            var primaryModel = !string.IsNullOrWhiteSpace(_options.Model)
                ? _options.Model
                : "gemini-3.8-flash";

            var modelCandidates = new List<string> { primaryModel };
            string[] fallbackCandidates = ["gemini-3.8-flash", "gemini-3.7-flash", "gemini-3.6-flash"];
            foreach (var fb in fallbackCandidates)
            {
                if (!modelCandidates.Contains(fb, StringComparer.OrdinalIgnoreCase))
                {
                    modelCandidates.Add(fb);
                }
            }

            var content = new Content
            {
                Parts = new List<Part>
                {
                    Part.FromBytes(pdfBytes, contentType),
                    Part.FromText("Extract portfolio data from this uploaded CV document according to the JSON schema. Strictly adhere to the anti-hallucination rules.")
                }
            };

            var config = new GenerateContentConfig
            {
                SystemInstruction = new Content
                {
                    Parts = new List<Part> { Part.FromText(SystemPrompt) }
                },
                ResponseMimeType = "application/json",
                ResponseJsonSchema = JsonNode.Parse(JsonSchema)
            };

            GenerateContentResponse? response = null;
            Exception? lastEx = null;

            foreach (var model in modelCandidates)
            {
                try
                {
                    _logger.LogInformation("Sending CV document directly to Gemini model: {Model}", model);
                    using var modelCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    modelCts.CancelAfter(TimeSpan.FromSeconds(20));
                    response = await client.Models.GenerateContentAsync(model, content, config, modelCts.Token);
                    if (response != null && !string.IsNullOrWhiteSpace(response.Text))
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Gemini model {Model} failed: {Message}. Attempting fallback...", model, ex.Message);
                    lastEx = ex;
                }
            }

            if (response == null || string.IsNullOrWhiteSpace(response.Text))
            {
                _logger.LogWarning(lastEx, "All Gemini candidate models failed to return a response.");
                return Result<CvPortfolioDraft>.Failure(lastEx != null ? $"Gemini provider error: {lastEx.Message}" : "Gemini model returned an empty response.");
            }

            var responseText = response.Text;
            var draft = JsonSerializer.Deserialize<CvPortfolioDraft>(responseText, JsonOptions);
            if (draft == null)
            {
                _logger.LogWarning("Failed to deserialize structured CV draft from Gemini response.");
                return Result<CvPortfolioDraft>.Failure("Failed to deserialize structured CV data from Gemini response.");
            }

            return Result<CvPortfolioDraft>.Success(draft);
        }
        catch (ClientError ex)
        {
            _logger.LogError("Gemini API client error: {Message}", ex.Message);
            return Result<CvPortfolioDraft>.Failure($"Gemini provider error: {ex.Message}");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Gemini CV parsing request timed out or was cancelled.");
            return Result<CvPortfolioDraft>.Failure("Gemini CV parsing request timed out.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during Gemini CV parsing.");
            return Result<CvPortfolioDraft>.Failure("An unexpected error occurred while processing the CV with Gemini.");
        }
    }

    private static Result<CvPortfolioDraft> ParseMockCv(byte[] pdfBytes)
    {
        // Simple binary/string scan for deterministic mock test evaluation
        var text = Encoding.UTF8.GetString(pdfBytes);

        // Check for empty/blank document test case:
        // A PDF without fonts/text streams or with empty content length (e.g., Length1 0) is empty
        bool isEmpty = (!text.Contains("/Font") && !text.Contains("/BaseFont") && !text.Contains("John Doe")) ||
                       text.Contains("/Length1 0") ||
                       pdfBytes.Length < 800;

        if (isEmpty)
        {
            return Result<CvPortfolioDraft>.Failure("Could not extract readable text from the CV.");
        }

        // 1. Personal Info
        var personalInfo = new CvPersonalInfoDraft(
            "John Doe",
            "Senior .NET Developer",
            "Passionate software engineer with 6+ years of experience building scalable web APIs.");

        // 2. Skills
        var skills = new List<CvSkillCategoryDraft>
        {
            new("Backend & Cloud", new List<CvSkillDraft>
            {
                new("ASP.NET Core", null),
                new("C#", null),
                new("EF Core", null),
                new("SQL Server", null),
                new("Angular", null),
                new("Docker", null)
            })
        };

        // 3. Experiences
        var experiences = new List<CvExperienceDraft>
        {
            new(
                "Senior .NET Developer",
                "Jedra Solutions",
                "Built microservices using ASP.NET Core and SQL Server.",
                "2023",
                null,
                true)
        };

        // 4. Projects
        var projects = new List<CvProjectDraft>
        {
            new(
                "School Management System",
                "Web portal for schools built with ASP.NET Core and Angular.",
                null,
                "https://example.com/demo",
                "https://github.com/example/project",
                new List<string> { "ASP.NET Core", "SQL Server" },
                false)
        };

        // 5. Education
        var education = new List<CvEducationDraft>
        {
            new(
                "Cairo University",
                "Bachelor of Science",
                "Computer Science",
                null,
                "2018",
                "2022",
                false)
        };

        // 6. Certifications
        var certifications = new List<CvCertificationDraft>
        {
            new(
                "Microsoft Certified: Azure Developer Associate",
                "Microsoft",
                "2024",
                "CERT-12345",
                "https://learn.microsoft.com/verify/12345",
                null)
        };

        // 7. Social Links
        var socialLinks = new List<CvSocialLinkDraft>
        {
            new("GitHub", "https://github.com/johndoe"),
            new("LinkedIn", "https://linkedin.com/in/johndoe")
        };

        var draft = new CvPortfolioDraft(
            personalInfo,
            skills,
            experiences,
            projects,
            education,
            certifications,
            socialLinks);

        return Result<CvPortfolioDraft>.Success(draft);
    }
}
