using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSUClassPath.Data;
using OSUClassPath.Filters;
using OSUClassPath.Models;

namespace OSUClassPath.Controllers;

public class AdminController : Controller
{
    private readonly AdvisorDbContext _context;
    private readonly IConfiguration _configuration;

    public AdminController(
        AdvisorDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [AdminOnly]
    public async Task<IActionResult> Index()
    {
        var users = await _context.Users
            .AsNoTracking()
            .GroupJoin(
                _context.StudentCourses.AsNoTracking(),
                user => user.Id,
                course => course.UserId,
                (user, courses) => new AdminUserSummaryViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    DisplayName = user.DisplayName,
                    Program = user.Program,
                    CatalogYear = user.CatalogYear,
                    AcademicYear = user.AcademicYear,
                    PreferredCredits = user.PreferredCredits,
                    SavedCourseCount = courses.Count()
                })
            .OrderBy(user => user.Email)
            .ToListAsync();

        return View(users);
    }

    [AdminOnly]
    public async Task<IActionResult> UserCourses(string id)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Where(user => user.Id == id)
            .Select(user => new AdminUserSummaryViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? "",
                DisplayName = user.DisplayName,
                Program = user.Program,
                CatalogYear = user.CatalogYear,
                AcademicYear = user.AcademicYear,
                PreferredCredits = user.PreferredCredits,
                SavedCourseCount = _context.StudentCourses.Count(course => course.UserId == user.Id)
            })
            .FirstOrDefaultAsync();

        if (user is null)
        {
            return NotFound();
        }

        var courses = await _context.StudentCourses
            .AsNoTracking()
            .Include(course => course.Course)
            .Where(course => course.UserId == id)
            .OrderBy(course => course.Course!.CourseCode)
            .ToListAsync();

        return View(new AdminUserCoursesViewModel
        {
            User = user,
            Courses = courses
        });
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = string.IsNullOrWhiteSpace(returnUrl) ? "/Admin" : returnUrl;
        ViewData["AdminPasswordConfigured"] = !string.IsNullOrWhiteSpace(GetAdminPassword());
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(string password, string? returnUrl = null)
    {
        var adminPassword = GetAdminPassword();

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            ModelState.AddModelError(string.Empty, "Admin password is not configured.");
            ViewData["ReturnUrl"] = returnUrl ?? "/Admin";
            ViewData["AdminPasswordConfigured"] = false;
            return View();
        }

        if (password == adminPassword)
        {
            HttpContext.Session.SetString("IsAdmin", "true");
            return LocalRedirect(IsLocalReturnUrl(returnUrl) ? returnUrl! : "/Admin");
        }

        ModelState.AddModelError(string.Empty, "Incorrect password.");
        ViewData["ReturnUrl"] = returnUrl ?? "/Admin";
        ViewData["AdminPasswordConfigured"] = true;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("IsAdmin");
        return RedirectToAction("Index", "Home");
    }

    private string? GetAdminPassword()
    {
        return _configuration["Admin:Password"]
            ?? Environment.GetEnvironmentVariable("OSUCOURSEPATH_ADMIN_PASSWORD");
    }

    private static bool IsLocalReturnUrl(string? returnUrl)
    {
        return !string.IsNullOrWhiteSpace(returnUrl)
            && Uri.TryCreate(returnUrl, UriKind.Relative, out _)
            && returnUrl.StartsWith('/');
    }
}
