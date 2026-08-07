using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OSUClassPath.Models;

namespace OSUClassPath.Data;

public static class CourseSeeder
{
    public static async Task SeedAsync(AdvisorDbContext dbContext, IWebHostEnvironment environment)
    {
        var seedPath = Path.Combine(environment.ContentRootPath, "Data", "SeedCourses.json");
        if (!File.Exists(seedPath))
        {
            return;
        }

        var seedJson = await File.ReadAllTextAsync(seedPath);
        var seedCourses = JsonSerializer.Deserialize<List<CourseSeedItem>>(seedJson) ?? [];

        foreach (var seedCourse in seedCourses)
        {
            var existingCourse = await dbContext.Courses
                .FirstOrDefaultAsync(course => course.CourseCode == seedCourse.CourseCode);

            if (existingCourse is null)
            {
                dbContext.Courses.Add(seedCourse.ToCourse());
                continue;
            }

            existingCourse.Category = seedCourse.Category;
            existingCourse.Track = seedCourse.Track;
            existingCourse.Title = seedCourse.Title;
            existingCourse.Description = seedCourse.Description;
            existingCourse.Credits = seedCourse.Credits;
            existingCourse.PrerequisiteText = seedCourse.PrerequisiteText;
            existingCourse.SourceUrl = seedCourse.SourceUrl;
            existingCourse.LastVerified = seedCourse.LastVerified;
        }

        await dbContext.SaveChangesAsync();
    }

    private sealed class CourseSeedItem
    {
        public string CourseCode { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Track { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Credits { get; set; }

        public string PrerequisiteText { get; set; } = string.Empty;

        public string SourceUrl { get; set; } = string.Empty;

        public DateTime LastVerified { get; set; } = DateTime.Today;

        public Course ToCourse()
        {
            return new Course
            {
                CourseCode = CourseCode,
                Category = Category,
                Track = Track,
                Title = Title,
                Description = Description,
                Credits = Credits,
                PrerequisiteText = PrerequisiteText,
                SourceUrl = SourceUrl,
                LastVerified = LastVerified
            };
        }
    }
}
