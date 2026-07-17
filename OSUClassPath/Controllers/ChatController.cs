using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using OSUClassPath.Data;
using System.Text.RegularExpressions;

namespace OSUClassPath.Controllers;

public class ChatController : Controller
{
    private readonly AdvisorDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        AdvisorDbContext context,
        IConfiguration configuration,
        ILogger<ChatController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// 將使用者訊息送至 OpenAI 大語言模型。
    /// </summary>
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
        if (request is null || string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { error = "Message 不可為空白。" });
        }

        var apiKey = _configuration["OpenAI:ApiKey"]
            ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogError("OpenAI API key 尚未設定。");
            return Problem(
                title: "伺服器尚未設定 OpenAI API key。",
                detail: "請設定 OpenAI:ApiKey 或 OPENAI_API_KEY。",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var model = _configuration["OpenAI:Model"] ?? "gpt-5.6-luna";
        var client = new ChatClient(model, apiKey);

        try
        {
            ChatCompletion completion = await client.CompleteChatAsync(
                [new UserChatMessage(request.Message.Trim())],
                new ChatCompletionOptions(),
                cancellationToken);

            var answer = string.Concat(completion.Content.Select(part => part.Text));

            return Ok(new ChatResponse(answer, model));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return StatusCode(499);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "呼叫 OpenAI API 時發生錯誤。");
            return Problem(
                title: "大語言模型暫時無法回應。",
                detail: "請稍後再試，或檢查 API key、模型名稱及帳戶額度。",
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
            ViewBag.Answer = "請輸入問題。";
            return View();
        }

        var match = Regex.Match(
            question.ToUpper(),
            @"\b[A-Z]{2,10}\s*\d{4}\b");

        if (!match.Success)
        {
            ViewBag.Answer =
                "我目前需要問題中包含課程代碼，例如 CSE 2221。";

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
            ViewBag.Answer =
                $"資料庫中目前找不到 {courseCode}。";

            return View();
        }

        ViewBag.Answer =
            $"{course.CourseCode} 是 {course.Title}，" +
            $"共 {course.Credits} 學分。\n\n" +
            $"課程介紹：{course.Description}\n\n" +
            $"先修條件：{course.PrerequisiteText}";

        ViewBag.SourceUrl = course.SourceUrl;

        return View();
    }

    public sealed record ChatRequest(string Message);

    public sealed record ChatResponse(string Answer, string Model);
}
