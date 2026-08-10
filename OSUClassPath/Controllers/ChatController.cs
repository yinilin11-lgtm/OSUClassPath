using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSUClassPath.Data;
using OSUClassPath.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace OSUClassPath.Controllers;

public class ChatController : Controller
{
    private const string NoCourseContext = "No matching course records found.";
    private const string NoScheduleContext = "No recommended schedule records found.";
    private const string NoProgramContext = "No BS CSE program requirement records found.";

    private readonly AdvisorDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ChatController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ChatController(
        AdvisorDbContext context,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger<ChatController> logger,
        IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost("/api/chat")]
    [Produces("application/json")]
    [ProducesResponseType<ChatResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> CompleteChat(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        return await CompleteAgentChat(request, cancellationToken);
    }

    [HttpPost("/api/agent/chat")]
    [HttpPost("/api/course-advisor/chat")]
    [Produces("application/json")]
    [ProducesResponseType<AgentChatResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> CompleteAgentChat(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { error = "Message is required." });
        }

        var apiKey = GetGeminiApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogError("Gemini API key is not configured.");
            return Problem(
                title: "Gemini API key is missing.",
                detail: "Set Gemini:ApiKey in user secrets, or set the GEMINI_API_KEY environment variable.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var model = _configuration["Gemini:Model"] ?? "gemini-3.5-flash";
        var conversationContext = BuildConversationContext(request.History);
        var courseContext = await BuildCourseContextAsync($"{conversationContext}\n{request.Message}", cancellationToken);
        var scheduleContext = await BuildScheduleContextAsync(cancellationToken);
        var programContext = await BuildProgramContextAsync(cancellationToken);
        var missingCourseGuidance = BuildMissingCourseGuidance(courseContext);
        var usedContext = courseContext != NoCourseContext
            || scheduleContext != NoScheduleContext
            || programContext != NoProgramContext;

        var systemPrompt =
            "You are an AI course planning agent for the OSU CoursePath ASP.NET app. " +
            "Answer in Traditional Chinese by default. Be concise, practical, and friendly. " +
            "Use the provided course catalog categories and tracks when relevant. " +
            "Use the provided BS CSE program requirement context for degree-credit and department background questions. " +
            "Do not add every available course option together as if all options are required. " +
            "Explain whether a course is a core requirement, core choice, capstone, math/science elective, or CSE technical elective. " +
            "Treat GE entries as overall General Education placeholders, not specific course recommendations. " +
            "If a course is not found in the local database, do not guess course details. " +
            "Explain that the local catalog may not include every OSU course, provide the official OSU CSE course catalog link when relevant, " +
            "and offer to help interpret degree requirements or possible category fit using the available program rules. " +
            "If the context is not enough, say what information is missing in a helpful way.";

        var userPrompt =
            $"Recent conversation:\n{conversationContext}\n\n" +
            $"Course context from the local database:\n{courseContext}\n\n" +
            $"Local catalog fallback guidance:\n{missingCourseGuidance}\n\n" +
            $"Recommended schedule context:\n{scheduleContext}\n\n" +
            $"BS CSE program requirement and department context:\n{programContext}\n\n" +
            $"Student question:\n{request.Message.Trim()}";

        try
        {
            var answer = await GenerateGeminiContentAsync(
                apiKey,
                model,
                systemPrompt,
                userPrompt,
                cancellationToken);
            var suggestedCourses = await BuildSuggestedCoursesAsync(
                $"{request.Message}\n{answer}",
                cancellationToken);

            return Ok(new AgentChatResponse(answer, model, usedContext, suggestedCourses));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return StatusCode(499);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Gemini API request failed.");
            var detail = _environment.IsDevelopment()
                ? GetUserFriendlyGeminiError(exception)
                : "Please check the Gemini model, API key, network connection, and account status.";

            return Problem(
                title: "AI agent request failed.",
                detail: detail,
                statusCode: StatusCodes.Status502BadGateway);
        }
    }
    private string? GetGeminiApiKey()
    {
        return _configuration["Gemini:ApiKey"]
            ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");
    }

    private async Task<string> GenerateGeminiContentAsync(
        string apiKey,
        string model,
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(model)}:generateContent");

        request.Headers.Add("x-goog-api-key", apiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = JsonContent.Create(new
        {
            systemInstruction = new
            {
                parts = new[]
                {
                    new { text = systemPrompt }
                }
            },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text = userPrompt }
                    }
                }
            }
        });

        using var response = await client.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Gemini API returned HTTP {(int)response.StatusCode}: {ExtractGeminiError(responseBody)}");
        }

        using var document = JsonDocument.Parse(responseBody);
        var root = document.RootElement;

        if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("Gemini API did not return any candidates.");
        }

        var builder = new StringBuilder();
        var parts = candidates[0]
            .GetProperty("content")
            .GetProperty("parts");

        foreach (var part in parts.EnumerateArray())
        {
            if (part.TryGetProperty("text", out var text))
            {
                builder.Append(text.GetString());
            }
        }

        var answer = builder.ToString();

        if (string.IsNullOrWhiteSpace(answer))
        {
            throw new InvalidOperationException("Gemini API returned an empty response.");
        }

        return answer;
    }

    private static string ExtractGeminiError(string responseBody)
    {
        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;

            if (root.TryGetProperty("error", out var error)
                && error.TryGetProperty("message", out var message))
            {
                return message.GetString() ?? responseBody;
            }
        }
        catch (JsonException)
        {
            return responseBody;
        }

        return responseBody;
    }

    private static string GetUserFriendlyGeminiError(Exception exception)
    {
        var message = exception.Message;

        if (message.Contains("API key not valid", StringComparison.OrdinalIgnoreCase)
            || message.Contains("invalid api key", StringComparison.OrdinalIgnoreCase))
        {
            return "Gemini API key is invalid. Please create a new API key in Google AI Studio and set Gemini:ApiKey with dotnet user-secrets.";
        }

        if (message.Contains("quota", StringComparison.OrdinalIgnoreCase)
            || message.Contains("rate limit", StringComparison.OrdinalIgnoreCase))
        {
            return "Gemini API quota or rate limit was reached. Please check your Google AI Studio or Google Cloud quota and billing settings.";
        }

        if (message.Contains("model", StringComparison.OrdinalIgnoreCase)
            && (message.Contains("not found", StringComparison.OrdinalIgnoreCase)
                || message.Contains("not supported", StringComparison.OrdinalIgnoreCase)))
        {
            return $"Gemini 璅∪?閮剖??航銝?具?憪隤歹?{message}";
        }

        return message;
    }

    private async Task<string> BuildCourseContextAsync(string message, CancellationToken cancellationToken)
    {
        var normalizedMessage = message.Trim().ToUpperInvariant();
        var courseCodes = Regex.Matches(normalizedMessage, @"\b[A-Z]{2,10}\s*\d{4}\b")
            .Select(match => Regex.Replace(match.Value, @"\s+", " "))
            .Distinct()
            .ToList();

        List<Course> courses;

        if (courseCodes.Count > 0)
        {
            courses = await _context.Courses
                .AsNoTracking()
                .Where(course => courseCodes.Contains(course.CourseCode))
                .OrderBy(course => course.CourseCode)
                .Take(8)
                .ToListAsync(cancellationToken);
        }
        else
        {
            var trackHints = GetTrackHints(normalizedMessage);
            var categoryHints = GetCategoryHints(normalizedMessage);
            var levelHints = GetCourseLevelHints(normalizedMessage);
            var hasStructuredHints = trackHints.Count > 0 || categoryHints.Count > 0 || levelHints.Count > 0;

            var keywords = Regex.Matches(normalizedMessage, @"[A-Z0-9]{3,}")
                .Select(match => match.Value)
                .Where(keyword => keyword is not ("THE" or "AND" or "FOR" or "WITH"))
                .Distinct()
                .Take(5)
                .ToList();

            var allCourses = await _context.Courses
                .AsNoTracking()
                .OrderBy(course => course.CourseCode)
                .ToListAsync(cancellationToken);

            courses = allCourses
                .Where(course =>
                    (trackHints.Count == 0 || trackHints.Contains(course.Track))
                    && (categoryHints.Count == 0 || categoryHints.Contains(course.Category))
                    && (levelHints.Count == 0 || MatchesCourseLevel(course.CourseCode, levelHints))
                    && (hasStructuredHints
                        || keywords.Count == 0
                        || keywords.Any(keyword =>
                            course.CourseCode.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                            || course.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                            || course.Category.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                            || course.Track.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                            || course.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                            || course.PrerequisiteText.Contains(keyword, StringComparison.OrdinalIgnoreCase))))
                .Take(hasStructuredHints ? 20 : 8)
                .ToList();
        }

        if (courses.Count == 0)
        {
            return NoCourseContext;
        }

        var contextBuilder = new StringBuilder();

        foreach (var course in courses)
        {
            contextBuilder.AppendLine($"- {course.CourseCode}: {course.Title}");
            contextBuilder.AppendLine($"  Category: {course.Category}");
            contextBuilder.AppendLine($"  Track: {course.Track}");
            contextBuilder.AppendLine($"  Credits: {course.Credits}");
            contextBuilder.AppendLine($"  Description: {course.Description}");
            contextBuilder.AppendLine($"  Prerequisites: {course.PrerequisiteText}");
            contextBuilder.AppendLine($"  Source: {course.SourceUrl}");
        }

        return contextBuilder.ToString();
    }

    private async Task<List<AgentSuggestedCourse>> BuildSuggestedCoursesAsync(
        string text,
        CancellationToken cancellationToken)
    {
        var normalizedText = text.Trim().ToUpperInvariant();
        var courseCodes = Regex.Matches(normalizedText, @"\b[A-Z]{2,10}\s*\d{4}[A-Z]?\b")
            .Select(match => Regex.Replace(match.Value, @"\s+", " "))
            .Distinct()
            .Take(8)
            .ToList();

        List<Course> courses;

        if (courseCodes.Count > 0)
        {
            courses = await _context.Courses
                .AsNoTracking()
                .Where(course => courseCodes.Contains(course.CourseCode))
                .OrderBy(course => course.CourseCode)
                .Take(8)
                .ToListAsync(cancellationToken);
        }
        else
        {
            var trackHints = GetTrackHints(normalizedText);
            var categoryHints = GetCategoryHints(normalizedText);

            if (trackHints.Count == 0 && categoryHints.Count == 0)
            {
                return [];
            }

            courses = await _context.Courses
                .AsNoTracking()
                .Where(course =>
                    (trackHints.Count == 0 || trackHints.Contains(course.Track))
                    && (categoryHints.Count == 0 || categoryHints.Contains(course.Category)))
                .OrderBy(course => course.CourseCode)
                .Take(8)
                .ToListAsync(cancellationToken);
        }

        return courses
            .Select(course => new AgentSuggestedCourse(
                course.Id,
                course.CourseCode,
                course.Title,
                course.Credits,
                course.Category,
                course.Track,
                course.PrerequisiteText))
            .ToList();
    }

    private static List<int> GetCourseLevelHints(string normalizedMessage)
    {
        var levels = new List<int>();

        foreach (Match match in Regex.Matches(normalizedMessage, @"\b([3-5])(?:000|XXX)\b"))
        {
            if (int.TryParse(match.Groups[1].Value, out var level) && !levels.Contains(level))
            {
                levels.Add(level);
            }
        }

        return levels;
    }

    private static bool MatchesCourseLevel(string courseCode, IReadOnlyCollection<int> levels)
    {
        var match = Regex.Match(courseCode, @"\b(\d{4})");
        if (!match.Success || !int.TryParse(match.Groups[1].Value, out var courseNumber))
        {
            return false;
        }

        return levels.Contains(courseNumber / 1000);
    }

    private static string BuildConversationContext(IReadOnlyList<ChatHistoryMessage>? history)
    {
        if (history is null || history.Count == 0)
        {
            return "No previous conversation in this browser session.";
        }

        var contextBuilder = new StringBuilder();

        foreach (var message in history.TakeLast(10))
        {
            if (string.IsNullOrWhiteSpace(message.Content))
            {
                continue;
            }

            var role = message.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase)
                ? "Assistant"
                : "Student";
            var content = message.Content.Trim();

            if (content.Length > 900)
            {
                content = content[..900] + "...";
            }

            contextBuilder.AppendLine($"{role}: {content}");
        }

        return contextBuilder.Length == 0
            ? "No previous conversation in this browser session."
            : contextBuilder.ToString();
    }

    private static string BuildMissingCourseGuidance(string courseContext)
    {
        if (courseContext != NoCourseContext)
        {
            return "Local course records were found. Use them as the primary source for course-specific details.";
        }

        return
            "No matching local course record was found. This does not mean the course does not exist at OSU. " +
            "The local catalog focuses on BS CSE core, core choices, math/science electives, technical elective candidates, and related planning data. " +
            "When answering, say that the course is not in the local OSU CoursePath catalog yet, avoid inventing title/credits/prerequisites, " +
            "and suggest checking the official OSU CSE course catalog: https://cse.osu.edu/courses. " +
            "If the student is asking whether it may count toward BS CSE requirements, use the program requirement rules to explain what would need to be verified.";
    }

    private static List<string> GetTrackHints(string normalizedMessage)
    {
        var hints = new List<string>();

        AddHint(hints, normalizedMessage, "Artificial Intelligence", "AI", "ARTIFICIAL", "MACHINE", "LEARNING", "NEURAL", "VISION", "NLP");
        AddHint(hints, normalizedMessage, "Game / Graphics", "GAME", "GAMING", "GRAPHICS", "ANIMATION", "RENDERING", "VISUALIZATION", "VR", "VIRTUAL");
        AddHint(hints, normalizedMessage, "Security", "SECURITY", "CYBER", "CRYPTO", "MALWARE", "HACK");
        AddHint(hints, normalizedMessage, "Database / Data", "DATABASE", "DATA", "MINING", "CLOUD");
        AddHint(hints, normalizedMessage, "Software Engineering", "SOFTWARE", "WEB", "MOBILE", "APP", "ENTERPRISE");
        AddHint(hints, normalizedMessage, "Networking", "NETWORK", "WIRELESS", "INTERNET");
        AddHint(hints, normalizedMessage, "Systems", "SYSTEM", "SYSTEMS", "OS", "OPERATING", "ARCHITECTURE", "PARALLEL", "COMPILER");
        AddHint(hints, normalizedMessage, "Theory / Algorithms", "THEORY", "ALGORITHM", "AUTOMATA", "FORMAL");
        AddHint(hints, normalizedMessage, "Math / Statistics", "MATH", "STAT", "STATISTICS");
        AddHint(hints, normalizedMessage, "Science", "SCIENCE", "BIOLOGY", "CHEM", "PHYSICS", "EARTH", "ENVIRONMENT");

        return hints;
    }

    private static List<string> GetCategoryHints(string normalizedMessage)
    {
        var hints = new List<string>();

        AddHint(hints, normalizedMessage, "CSE Technical Elective", "TECHNICAL ELECTIVE", "ELECTIVE", "TRACK");
        AddHint(hints, normalizedMessage, "Computer Science Core", "CORE", "REQUIRED");
        AddHint(hints, normalizedMessage, "Computer Science Core Choices", "CORE CHOICE", "CAPSTONE");
        AddHint(hints, normalizedMessage, "CSE Math and Science Electives", "MATH", "SCIENCE");
        AddHint(hints, normalizedMessage, "Non-Computer Science Core", "NON-CSE", "ECE");

        return hints;
    }

    private static void AddHint(List<string> hints, string message, string value, params string[] tokens)
    {
        if (tokens.Any(token => message.Contains(token, StringComparison.OrdinalIgnoreCase)) && !hints.Contains(value))
        {
            hints.Add(value);
        }
    }

    private async Task<string> BuildScheduleContextAsync(CancellationToken cancellationToken)
    {
        var terms = await _context.RecommendedPlanTerms
            .AsNoTracking()
            .Include(term => term.Items.OrderBy(item => item.SortOrder))
            .OrderBy(term => term.SortOrder)
            .ToListAsync(cancellationToken);

        if (terms.Count == 0)
        {
            return NoScheduleContext;
        }

        var contextBuilder = new StringBuilder();

        foreach (var term in terms)
        {
            contextBuilder.AppendLine($"{term.DisplayName} ({term.RecommendedCredits} credits):");

            foreach (var item in term.Items.OrderBy(item => item.SortOrder))
            {
                var typeLabel = item.ItemType switch
                {
                    RecommendedPlanItemType.GeneralEducation => "GE placeholder",
                    RecommendedPlanItemType.Elective => "Elective",
                    RecommendedPlanItemType.Requirement => "Requirement choice",
                    _ => "Course"
                };

                var notes = string.IsNullOrWhiteSpace(item.Notes)
                    ? string.Empty
                    : $" Notes: {item.Notes}";

                contextBuilder.AppendLine($"- {item.CourseCode}: {item.Title}, {item.Credits} credits, {typeLabel}.{notes}");
            }
        }

        return contextBuilder.ToString();
    }

    private async Task<string> BuildProgramContextAsync(CancellationToken cancellationToken)
    {
        var seedPath = Path.Combine(_environment.ContentRootPath, "Data", "CseProgramRequirements.json");

        if (!System.IO.File.Exists(seedPath))
        {
            return NoProgramContext;
        }

        try
        {
            await using var stream = System.IO.File.OpenRead(seedPath);
            var program = await JsonSerializer.DeserializeAsync<CseProgramRequirementContext>(
                stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken);

            if (program is null)
            {
                return NoProgramContext;
            }

            var contextBuilder = new StringBuilder();
            contextBuilder.AppendLine($"{program.ShortName}: {program.ProgramName}");
            contextBuilder.AppendLine($"Institution: {program.Institution}");
            contextBuilder.AppendLine($"College: {program.College}");
            contextBuilder.AppendLine($"Department: {program.Department}");
            contextBuilder.AppendLine($"Last verified: {program.LastVerified}");

            if (program.Background.Count > 0)
            {
                contextBuilder.AppendLine("Department/program background:");
                foreach (var item in program.Background)
                {
                    contextBuilder.AppendLine($"- {item}");
                }
            }

            if (program.Requirements.Count > 0)
            {
                contextBuilder.AppendLine("Degree requirement summary:");
                foreach (var requirement in program.Requirements)
                {
                    contextBuilder.AppendLine($"- {requirement.Name}: {requirement.RequiredCredits} required credits.");
                    contextBuilder.AppendLine($"  Description: {requirement.Description}");

                    if (requirement.MinimumCseCredits is not null)
                    {
                        contextBuilder.AppendLine($"  Minimum CSE credits: {requirement.MinimumCseCredits}");
                    }

                    foreach (var note in requirement.Notes)
                    {
                        contextBuilder.AppendLine($"  Note: {note}");
                    }
                }
            }

            if (program.CoreChoiceGroups.Count > 0)
            {
                contextBuilder.AppendLine("CSE core choice groups:");
                foreach (var group in program.CoreChoiceGroups)
                {
                    contextBuilder.AppendLine($"- {group.Name}: choose {group.Choose} from {string.Join(", ", group.Courses)}.");
                }
            }

            if (program.TechnicalElectiveRules.Count > 0)
            {
                contextBuilder.AppendLine("Technical elective rules:");
                foreach (var rule in program.TechnicalElectiveRules)
                {
                    contextBuilder.AppendLine($"- {rule}");
                }
            }

            if (program.SpecializationOptions.Count > 0)
            {
                contextBuilder.AppendLine("Specialization options:");
                foreach (var option in program.SpecializationOptions)
                {
                    contextBuilder.AppendLine($"- {option.Name}: {option.Summary}");

                    foreach (var requirement in option.Requirements)
                    {
                        contextBuilder.AppendLine($"  Requirement/note: {requirement}");
                    }
                }
            }

            if (program.AdvisingNotes.Count > 0)
            {
                contextBuilder.AppendLine("Advising notes:");
                foreach (var note in program.AdvisingNotes)
                {
                    contextBuilder.AppendLine($"- {note}");
                }
            }

            if (program.Sources.Count > 0)
            {
                contextBuilder.AppendLine("Sources:");
                foreach (var source in program.Sources)
                {
                    contextBuilder.AppendLine($"- {source}");
                }
            }

            return contextBuilder.ToString();
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(exception, "BS CSE program requirement seed file could not be parsed.");
            return NoProgramContext;
        }
    }

    public sealed record ChatRequest(string Message, IReadOnlyList<ChatHistoryMessage>? History = null);

    public sealed record ChatHistoryMessage(string Role, string Content);

    public sealed record ChatResponse(string Answer, string Model);

    public sealed record AgentChatResponse(
        string Answer,
        string Model,
        bool UsedCourseContext,
        IReadOnlyList<AgentSuggestedCourse> SuggestedCourses);

    public sealed record AgentSuggestedCourse(
        int Id,
        string CourseCode,
        string Title,
        int Credits,
        string Category,
        string Track,
        string PrerequisiteText);

    private sealed record CseProgramRequirementContext(
        string ProgramName,
        string ShortName,
        string Institution,
        string College,
        string Department,
        string LastVerified,
        IReadOnlyList<string> Sources,
        IReadOnlyList<string> Background,
        IReadOnlyList<CseProgramRequirement> Requirements,
        IReadOnlyList<CseCoreChoiceGroup> CoreChoiceGroups,
        IReadOnlyList<string> TechnicalElectiveRules,
        IReadOnlyList<CseSpecializationOption> SpecializationOptions,
        IReadOnlyList<string> AdvisingNotes);

    private sealed record CseProgramRequirement(
        string Name,
        int RequiredCredits,
        string Description,
        int? MinimumCseCredits,
        IReadOnlyList<string> Notes);

    private sealed record CseCoreChoiceGroup(
        string Name,
        int Choose,
        IReadOnlyList<string> Courses);

    private sealed record CseSpecializationOption(
        string Name,
        string Summary,
        IReadOnlyList<string> Requirements);
}
