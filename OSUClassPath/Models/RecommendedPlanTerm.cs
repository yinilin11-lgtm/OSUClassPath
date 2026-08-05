using System.ComponentModel.DataAnnotations;

namespace OSUClassPath.Models;

public class RecommendedPlanTerm
{
    public int Id { get; set; }

    [Display(Name = "年級")]
    public int YearNumber { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "學期")]
    public string TermName { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    [Display(Name = "顯示名稱")]
    public string DisplayName { get; set; } = string.Empty;

    [Display(Name = "建議總學分")]
    public int RecommendedCredits { get; set; }

    [Display(Name = "排序")]
    public int SortOrder { get; set; }

    public ICollection<RecommendedPlanItem> Items { get; set; } = new List<RecommendedPlanItem>();
}
