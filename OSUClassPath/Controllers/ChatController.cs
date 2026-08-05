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
        var courseContext = await BuildCourseContextAsync(request.Message, cancellationToken);
        var scheduleContext = await BuildScheduleContextAsync(cancellationToken);
        var usedContext = courseContext != NoCourseContext || scheduleContext != NoScheduleContext;

        var systemPrompt =
            "You are an AI course planning agent for the OSUClassPath ASP.NET app. " +
            "Answer in Traditional Chinese by default. Be concise, practical, and friendly. " +
            "Use the provided course catalog and recommended schedule context when relevant. " +
            "Treat GE entries as overall General Education placeholders, not specific course recommendations. " +
            "If the context is not enough, say what information is missing.";

        var userPrompt =
            $"Course context from the local database:\n{courseContext}\n\n" +
            $"Recommended schedule context:\n{scheduleContext}\n\n" +
            $"Student question:\n{request.Message.Trim()}";

        try
        {
            var answer = await GenerateGeminiContentAsync(
                apiKey,
                model,
                systemPrompt,
                userPrompt,
                cancellationToken);

            return Ok(new AgentChatResponse(answer, model, usedContext));
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
                .Where(course => keywords.Count == 0
                    || keywords.Any(keyword =>
                        course.CourseCode.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                        || course.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                        || course.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                        || course.PrerequisiteText.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                .Take(8)
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
            contextBuilder.AppendLine($"  Credits: {course.Credits}");
            contextBuilder.AppendLine($"  Description: {course.Description}");
            contextBuilder.AppendLine($"  Prerequisites: {course.PrerequisiteText}");
            contextBuilder.AppendLine($"  Source: {course.SourceUrl}");
        }

        return contextBuilder.ToString();
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

    public sealed record ChatRequest(string Message);

    public sealed record ChatResponse(string Answer, string Model);

    public sealed record AgentChatResponse(string Answer, string Model, bool UsedCourseContext);
}
