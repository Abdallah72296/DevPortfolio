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
        10. If a field or detail cannot be confidently extracted, leave it null or empty. Never substitute today's date or placeholder text.
        11. Search the entire CV, especially the header, contact block, and footer, for contact details. Extract a complete email address and phone/mobile number when present, including international prefixes, spaces, parentheses, and hyphens; preserve the number as written. Treat labels such as Phone, Mobile, Telephone, Cell, WhatsApp, Contact, Address, Location, Based in, and City as clues to the corresponding field. Put a city/country or explicitly stated work location in location; do not mistake an employer or university address for the candidate's location, and never infer location from a phone number or country code. If a value is absent or ambiguous, return null.
        12. Format known dates as strict ISO-8601 'yyyy-MM-dd'; use the first day of the stated month, or January 1 when only a year is known. If a date is absent, return null. If current job or study is explicit, set isCurrent=true and endDate=null.
        13. Extract all education, including degree, institution, field of study, and relevant graduation project details in the description. Do not create a graduation project as a separate project unless the CV describes it as one.
        14. Extract all work experiences, including jobs, internships, freelance, and teaching roles, with responsibilities only when stated.
        15. Extract projects only when the CV identifies an actual project; technologies and URLs must be explicitly associated with that project.
        16. Extract skills, certifications, and professional profile links (such as GitHub and LinkedIn) only when stated in the CV.
        17. Map ONLY to the supported sections: PersonalInfo (name, title, summary, email, phone, location), Skills, Experiences, Projects, Education, Certifications, SocialLinks.
        18. Output MUST strictly adhere to the provided JSON schema.
        19. personalInfo.jobTitle MUST be a short, concise professional title (e.g. '.NET Backend Developer', 'Full Stack Developer', 'Software Engineer', max 3-5 words). NEVER put an executive summary, bio, or sentences into jobTitle. All summary and description text MUST go into personalInfo.about.
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
                "about": { "type": ["string", "null"] },
                "email": { "type": ["string", "null"] },
                "phone": { "type": ["string", "null"] },
                "location": { "type": ["string", "null"] }
              },
              "required": ["name", "jobTitle", "about", "email", "phone", "location"]
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
                : "gemini-flash-latest";

            var modelCandidates = new List<string> { primaryModel };
            string[] fallbackCandidates = ["gemini-flash-latest", "gemini-3.5-flash", "gemini-flash-lite-latest", "gemini-3.8-flash"];
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
                    modelCts.CancelAfter(TimeSpan.FromSeconds(60));
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
        _ = pdfBytes;
        return Result<CvPortfolioDraft>.Failure(
            "CV extraction is unavailable while AI:Provider is set to Mock or Test. Configure Gemini to parse uploaded CVs.");
    }
}
