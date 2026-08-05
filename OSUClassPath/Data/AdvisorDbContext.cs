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

    public DbSet<RecommendedPlanTerm> RecommendedPlanTerms => Set<RecommendedPlanTerm>();

    public DbSet<RecommendedPlanItem> RecommendedPlanItems => Set<RecommendedPlanItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RecommendedPlanTerm>()
            .HasMany(term => term.Items)
            .WithOne(item => item.RecommendedPlanTerm)
            .HasForeignKey(item => item.RecommendedPlanTermId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RecommendedPlanTerm>()
            .HasIndex(term => term.SortOrder)
            .IsUnique();

        modelBuilder.Entity<RecommendedPlanItem>()
            .HasIndex(item => new { item.RecommendedPlanTermId, item.SortOrder })
            .IsUnique();
    }
}
