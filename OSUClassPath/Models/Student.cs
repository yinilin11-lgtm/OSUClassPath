using System.ComponentModel.DataAnnotations;

namespace OSUClassPath.Models;

public class Student
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "學生姓名")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "學位")]
    public string Program { get; set; } = "BS CSE";

    [Range(2000, 2100)]
    [Display(Name = "入學年度")]
    public int CatalogYear { get; set; } = 2026;

    [Display(Name = "入學學期")]
    public string StartingTerm { get; set; } = "Autumn";

    [Range(1, 30)]
    [Display(Name = "每學期偏好學分")]
    public int PreferredCredits { get; set; } = 15;
}