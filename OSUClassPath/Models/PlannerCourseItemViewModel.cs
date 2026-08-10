namespace OSUClassPath.Models;

public class PlannerCourseItemViewModel
{
    public int Id { get; set; }

    public string CourseCode { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public int Credits { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Track { get; set; } = string.Empty;

    public string PrerequisiteText { get; set; } = string.Empty;
}
