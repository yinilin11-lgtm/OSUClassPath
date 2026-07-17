
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSUClassPath.Models;
using OSUClassPath.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

public class StudentCoursesController : Controller
{
    private readonly AdvisorDbContext _context;

    public StudentCoursesController(AdvisorDbContext context)
    {
        _context = context;
    }

    // GET: STUDENTCOURSES
    public async Task<IActionResult> Index()
    {
        var studentCourses = _context.StudentCourses
            .Include(record => record.Student)
            .Include(record => record.Course);

        return View(await studentCourses.ToListAsync());
    }

    // GET: STUDENTCOURSES/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var studentcourse = await _context.StudentCourses
            .FirstOrDefaultAsync(m => m.Id == id);
        if (studentcourse == null)
        {
            return NotFound();
        }

        return View(studentcourse);
    }

    // GET: STUDENTCOURSES/Create
    public IActionResult Create()
    {
        ViewData["StudentId"] = new SelectList(
            _context.Students,
            "Id",
            "Name");

        ViewData["CourseId"] = new SelectList(
            _context.Courses,
            "Id",
            "CourseCode");

        return View();
    }

    // POST: STUDENTCOURSES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,StudentId,Student,CourseId,Course,Status,Term,Grade")] StudentCourse studentcourse)
    {
        if (ModelState.IsValid)
        {
            _context.Add(studentcourse);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["StudentId"] = new SelectList(_context.Students, "Id", "Name");
        ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "CourseCode");
        return View(studentcourse);
    }

    // GET: STUDENTCOURSES/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var studentcourse = await _context.StudentCourses.FindAsync(id);
        if (studentcourse == null)
        {
            return NotFound();
        }
        ViewData["StudentId"] = new SelectList(_context.Students, "Id", "Name");
        ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "CourseCode");
        return View(studentcourse);
    }

    // POST: STUDENTCOURSES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,StudentId,Student,CourseId,Course,Status,Term,Grade")] StudentCourse studentcourse)
    {
        if (id != studentcourse.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(studentcourse);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentCourseExists(studentcourse.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(studentcourse);
    }

    // GET: STUDENTCOURSES/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var studentcourse = await _context.StudentCourses
            .FirstOrDefaultAsync(m => m.Id == id);
        if (studentcourse == null)
        {
            return NotFound();
        }

        return View(studentcourse);
    }

    // POST: STUDENTCOURSES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var studentcourse = await _context.StudentCourses.FindAsync(id);
        if (studentcourse != null)
        {
            _context.StudentCourses.Remove(studentcourse);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool StudentCourseExists(int? id)
    {
        return _context.StudentCourses.Any(e => e.Id == id);
    }
}
