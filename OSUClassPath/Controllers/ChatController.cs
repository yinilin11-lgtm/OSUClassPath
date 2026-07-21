using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using OSUClassPath.Data;
using OSUClassPath.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace OSUClassPath.Controllers;

public class ChatController : Controller
{
    private readonly AdvisorDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        AdvisorDbContext context,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger<ChatController> logger)
    {
        _context = context;
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    [HttpGet]
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

        var apiKey = GetOpenAiApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogError("OpenAI API key is not configured.");
            return Problem(
                title: "OpenAI API key is missing.",
                detail: "Set OpenAI:ApiKey in user secrets, or set the OPENAI_API_KEY environment variable.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var model = _configuration["OpenAI:Model"] ?? "gpt-5.6-sol";
        var client = new ChatClient(model, apiKey);
        var courseContext = await BuildCourseContextAsync(request.Message, cancellationToken);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(
                "You are an AI course planning agent for the OSUClassPath ASP.NET app. " +
                "Answer in Traditional Chinese by default. Be concise, practical, and friendly. " +
                "Use the provided course context when it is relevant. If the context is not enough, say what information is missing."),
            new UserChatMessage(
                $"Course context from the local database:\n{courseContext}\n\n" +
                $"Student question:\n{request.Message.Trim()}")
        };

        try
        {
            ChatCompletion completion = await client.CompleteChatAsync(
                messages,
                new ChatCompletionOptions(),
                cancellationToken);

            var answer = string.Concat(completion.Content.Select(part => part.Text));

            return Ok(new AgentChatResponse(answer, model, courseContext != "No matching course records found."));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return StatusCode(499);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "OpenAI API request failed.");
            var detail = _environment.IsDevelopment()
                ? GetUserFriendlyOpenAiError(exception)
                : "Please check the model, API key, network connection, and OpenAI account status.";

            return Problem(
                title: "AI agent request failed.",
                detail: detail,
                statusCode: StatusCodes.Status502BadGateway);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(string question)
    {
        ViewBag.Question = question;

        if (string.IsNullOrWhiteSpace(question))
        {
            ViewBag.Answer = "請輸入一個課程問題。";
            return View();
        }

        var match = Regex.Match(
            question.ToUpperInvariant(),
            @"\b[A-Z]{2,10}\s*\d{4}\b");

        if (!match.Success)
        {
            ViewBag.Answer = "我目前可以辨識課程代碼，例如 CSE 2221。";
            return View();
        }

        var rawCode = Regex.Replace(match.Value, @"\s+", "");
        var department = Regex.Match(rawCode, @"^[A-Z]+").Value;
        var number = Regex.Match(rawCode, @"\d{4}$").Value;
        var courseCode = $"{department} {number}";

        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.CourseCode == courseCode);

        if (course == null)
        {
            ViewBag.Answer = $"資料庫裡找不到 {courseCode}。";
            return View();
        }

        ViewBag.Answer =
            $"{course.CourseCode} - {course.Title}\n" +
            $"學分：{course.Credits}\n\n" +
            $"課程介紹：{course.Description}\n\n" +
            $"先修條件：{course.PrerequisiteText}";

        ViewBag.SourceUrl = course.SourceUrl;

        return View();
    }

    private string? GetOpenAiApiKey()
    {
        return _configuration["OpenAI:ApiKey"]
            ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    }

    private static string GetUserFriendlyOpenAiError(Exception exception)
    {
        var message = exception.Message;

        if (message.Contains("insufficient_quota", StringComparison.OrdinalIgnoreCase)
            || message.Contains("exceeded your current quota", StringComparison.OrdinalIgnoreCase))
        {
            return "OpenAI API 目前沒有可用額度。請到 OpenAI Platform 確認 billing、credit 或 usage limit，補上額度後重新送出問題即可。";
        }

        if (message.Contains("invalid_api_key", StringComparison.OrdinalIgnoreCase)
            || message.Contains("incorrect api key", StringComparison.OrdinalIgnoreCase))
        {
            return "OpenAI API key 無效。請重新建立 API key，並用 dotnet user-secrets 更新 OpenAI:ApiKey。";
        }

        if (message.Contains("model", StringComparison.OrdinalIgnoreCase)
            && message.Contains("not", StringComparison.OrdinalIgnoreCase))
        {
            return $"OpenAI 模型設定可能不可用。原始錯誤：{message}";
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
            return "No matching course records found.";
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

    public sealed record ChatRequest(string Message);

    public sealed record ChatResponse(string Answer, string Model);

    public sealed record AgentChatResponse(string Answer, string Model, bool UsedCourseContext);
}
