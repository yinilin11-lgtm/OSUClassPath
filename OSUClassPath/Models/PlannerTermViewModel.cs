namespace OSUClassPath.Models;

public class PlannerTermViewModel
{
    public string DisplayName { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public int RecommendedCredits { get; set; }

    public List<string> CourseCodes { get; set; } = [];
}
