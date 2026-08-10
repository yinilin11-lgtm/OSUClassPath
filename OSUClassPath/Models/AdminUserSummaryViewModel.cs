namespace OSUClassPath.Models;

public class AdminUserSummaryViewModel
{
    public string UserId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Program { get; set; } = string.Empty;

    public int CatalogYear { get; set; }

    public int PreferredCredits { get; set; }

    public int SavedCourseCount { get; set; }
}
