using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OSUClassPath.Models;

namespace OSUClassPath.Data;

public class AdvisorDbContext : IdentityDbContext<ApplicationUser>
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

    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();

    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

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

        modelBuilder.Entity<StudentCourse>()
            .HasOne(record => record.User)
            .WithMany()
            .HasForeignKey(record => record.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChatSession>()
            .HasOne(session => session.User)
            .WithMany()
            .HasForeignKey(session => session.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ChatMessage>()
            .HasOne(message => message.ChatSession)
            .WithMany(session => session.Messages)
            .HasForeignKey(message => message.ChatSessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
