using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OSUClassPath.Data;
using OSUClassPath.Models;

[Authorize]
public class StudentCoursesController : Controller
{
    private readonly AdvisorDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentCoursesController(
        AdvisorDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var studentCourses = await _context.StudentCourses
            .Include(record => record.Course)
            .Where(record => record.UserId == userId)
            .OrderBy(record => record.Course!.CourseCode)
            .ToListAsync();

        return View(studentCourses);
    }

    public async Task<IActionResult> Details(int? id)
    {
        var studentCourse = await FindUserCourseAsync(id);

        if (studentCourse is null)
        {
            return NotFound();
        }

        return View(studentCourse);
    }

    public IActionResult Create(int? courseId = null)
    {
        PopulateCourses(courseId);
        return View(new StudentCourse { CourseId = courseId ?? 0, Status = CourseStatus.Planned });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CourseId,Status,Term,Grade")] StudentCourse studentCourse)
    {
        if (ModelState.IsValid)
        {
            studentCourse.UserId = _userManager.GetUserId(User);
            _context.Add(studentCourse);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        PopulateCourses(studentCourse.CourseId);
        return View(studentCourse);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        var studentCourse = await FindUserCourseAsync(id);

        if (studentCourse is null)
        {
            return NotFound();
        }

        PopulateCourses(studentCourse.CourseId);
        return View(studentCourse);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("CourseId,Status,Term,Grade")] StudentCourse updatedCourse)
    {
        var studentCourse = await FindUserCourseAsync(id);

        if (studentCourse is null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            studentCourse.CourseId = updatedCourse.CourseId;
            studentCourse.Status = updatedCourse.Status;
            studentCourse.Term = updatedCourse.Term;
            studentCourse.Grade = updatedCourse.Grade;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        PopulateCourses(updatedCourse.CourseId);
        return View(updatedCourse);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        var studentCourse = await FindUserCourseAsync(id);

        if (studentCourse is null)
        {
            return NotFound();
        }

        return View(studentCourse);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var studentCourse = await FindUserCourseAsync(id);

        if (studentCourse is not null)
        {
            _context.StudentCourses.Remove(studentCourse);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<StudentCourse?> FindUserCourseAsync(int? id)
    {
        if (id is null)
        {
            return null;
        }

        var userId = _userManager.GetUserId(User);
        return await _context.StudentCourses
            .Include(record => record.Course)
            .FirstOrDefaultAsync(record => record.Id == id && record.UserId == userId);
    }

    private void PopulateCourses(int? selectedCourseId = null)
    {
        ViewData["CourseId"] = new SelectList(
            _context.Courses.OrderBy(course => course.CourseCode),
            "Id",
            "CourseCode",
            selectedCourseId);
    }
}
