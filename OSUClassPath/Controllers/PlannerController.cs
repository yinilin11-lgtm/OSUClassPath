using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSUClassPath.Data;
using OSUClassPath.Models;

namespace OSUClassPath.Controllers;

public class PlannerController : Controller
{
    private readonly AdvisorDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PlannerController(
        AdvisorDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var courses = await _context.Courses
            .AsNoTracking()
            .OrderBy(course => course.CourseCode)
            .Select(course => new PlannerCourseItemViewModel
            {
                Id = course.Id,
                CourseCode = course.CourseCode,
                Title = course.Title,
                Credits = course.Credits,
                Category = course.Category,
                Track = course.Track,
                PrerequisiteText = course.PrerequisiteText
            })
            .ToListAsync();

        var terms = await _context.RecommendedPlanTerms
            .AsNoTracking()
            .Include(term => term.Items)
            .OrderBy(term => term.SortOrder)
            .Select(term => new PlannerTermViewModel
            {
                DisplayName = term.DisplayName,
                SortOrder = term.SortOrder,
                RecommendedCredits = term.RecommendedCredits,
                CourseCodes = term.Items
                    .Where(item => item.CourseCode != null && !item.CourseCode.Contains('/'))
                    .OrderBy(item => item.SortOrder)
                    .Select(item => item.CourseCode!)
                    .ToList()
            })
            .ToListAsync();

        if (terms.Count == 0)
        {
            terms = BuildDefaultTerms();
        }

        var userId = _userManager.GetUserId(User);
        var completedCourseCodes = new List<string>();
        int? academicYear = null;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            var user = await _userManager.GetUserAsync(User);
            academicYear = user?.AcademicYear;

            completedCourseCodes = await _context.StudentCourses
                .AsNoTracking()
                .Include(record => record.Course)
                .Where(record =>
                    record.UserId == userId
                    && (record.Status == CourseStatus.Completed || record.Status == CourseStatus.Transferred)
                    && record.Course != null)
                .Select(record => record.Course!.CourseCode)
                .ToListAsync();
        }

        return View(new PlannerIndexViewModel
        {
            Courses = courses,
            Terms = terms,
            AcademicYear = academicYear,
            CompletedCourseCodes = completedCourseCodes
        });
    }

    private static List<PlannerTermViewModel> BuildDefaultTerms()
    {
        var names = new[]
        {
            "Year 1 Autumn",
            "Year 1 Spring",
            "Year 2 Autumn",
            "Year 2 Spring",
            "Year 3 Autumn",
            "Year 3 Spring",
            "Year 4 Autumn",
            "Year 4 Spring"
        };

        return names
            .Select((name, index) => new PlannerTermViewModel
            {
                DisplayName = name,
                SortOrder = index + 1,
                RecommendedCredits = 15
            })
            .ToList();
    }
}
