namespace OSUClassPath.Models;

public class PlannerIndexViewModel
{
    public List<PlannerCourseItemViewModel> Courses { get; set; } = [];

    public List<PlannerTermViewModel> Terms { get; set; } = [];

    public int? AcademicYear { get; set; }

    public List<string> CompletedCourseCodes { get; set; } = [];
}
