
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OSUClassPath.Models;
using OSUClassPath.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

public class CoursesController : Controller
{
    private readonly AdvisorDbContext _context;

    public CoursesController(AdvisorDbContext context)
    {
        _context = context;
    }

    // GET: COURSES
    public async Task<IActionResult> Index(string? searchString, string? category, string? track)
    {
        var courses = _context.Courses.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            courses = courses.Where(course =>
                course.CourseCode.Contains(searchString) ||
                course.Title.Contains(searchString) ||
                course.Category.Contains(searchString) ||
                course.Track.Contains(searchString));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            courses = courses.Where(course => course.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(track))
        {
            courses = courses.Where(course => course.Track == track);
        }

        ViewData["CurrentFilter"] = searchString;
        ViewData["CurrentCategory"] = category;
        ViewData["CurrentTrack"] = track;
        ViewData["TotalCourses"] = await _context.Courses.CountAsync();
        ViewData["TechnicalElectives"] = await _context.Courses.CountAsync(course => course.Category == "CSE Technical Elective");
        ViewData["TrackCount"] = await _context.Courses
            .Where(course => course.Track != "")
            .Select(course => course.Track)
            .Distinct()
            .CountAsync();
        ViewData["FeaturedTracks"] = await _context.Courses
            .AsNoTracking()
            .Where(course => course.Track != "")
            .GroupBy(course => course.Track)
            .Select(group => new TrackSummary(group.Key, group.Count()))
            .OrderBy(summary => summary.TrackName)
            .ToListAsync();
        ViewData["Categories"] = new SelectList(await _context.Courses
            .AsNoTracking()
            .Where(course => course.Category != "")
            .Select(course => course.Category)
            .Distinct()
            .OrderBy(value => value)
            .ToListAsync(), category);
        ViewData["Tracks"] = new SelectList(await _context.Courses
            .AsNoTracking()
            .Where(course => course.Track != "")
            .Select(course => course.Track)
            .Distinct()
            .OrderBy(value => value)
            .ToListAsync(), track);

        return View(await courses
            .OrderBy(course => course.CourseCode)
            .ToListAsync());
    }

    // GET: COURSES/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var course = await _context.Courses
            .FirstOrDefaultAsync(m => m.Id == id);
        if (course == null)
        {
            return NotFound();
        }

        return View(course);
    }

    // GET: COURSES/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: COURSES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,CourseCode,Category,Track,Title,Description,Credits,PrerequisiteText,SourceUrl,LastVerified")] Course course)
    {
        if (ModelState.IsValid)
        {
            _context.Add(course);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(course);
    }

    // GET: COURSES/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound();
        }
        return View(course);
    }

    // POST: COURSES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,CourseCode,Category,Track,Title,Description,Credits,PrerequisiteText,SourceUrl,LastVerified")] Course course)
    {
        if (id != course.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(course);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(course.Id))
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
        return View(course);
    }

    // GET: COURSES/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var course = await _context.Courses
            .FirstOrDefaultAsync(m => m.Id == id);
        if (course == null)
        {
            return NotFound();
        }

        return View(course);
    }

    // POST: COURSES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course != null)
        {
            _context.Courses.Remove(course);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CourseExists(int? id)
    {
        return _context.Courses.Any(e => e.Id == id);
    }

    public sealed record TrackSummary(string TrackName, int CourseCount);
}
