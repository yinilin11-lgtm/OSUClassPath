using System.ComponentModel.DataAnnotations;

namespace OSUClassPath.Models;

public class ProfileViewModel
{
    [Required]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [StringLength(30)]
    public string Program { get; set; } = "BS CSE";

    [Range(2000, 2100)]
    public int CatalogYear { get; set; } = 2026;

    [Range(1, 6)]
    public int AcademicYear { get; set; } = 1;

    [StringLength(30)]
    public string StartingTerm { get; set; } = "Autumn";

    [Range(1, 30)]
    public int PreferredCredits { get; set; } = 15;
}
