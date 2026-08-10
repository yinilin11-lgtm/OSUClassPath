namespace OSUClassPath.Models;

public class AdminUserCoursesViewModel
{
    public AdminUserSummaryViewModel User { get; set; } = new();

    public IReadOnlyList<StudentCourse> Courses { get; set; } = [];
}
