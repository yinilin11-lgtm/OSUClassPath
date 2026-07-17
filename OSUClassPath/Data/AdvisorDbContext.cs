using Microsoft.EntityFrameworkCore;
using OSUClassPath.Models;

namespace OSUClassPath.Data;

public class AdvisorDbContext : DbContext
{
    public AdvisorDbContext(DbContextOptions<AdvisorDbContext> options)
        : base(options)
    {
    }

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Student> Students => Set<Student>();

    public DbSet<StudentCourse> StudentCourses => Set<StudentCourse>();
}