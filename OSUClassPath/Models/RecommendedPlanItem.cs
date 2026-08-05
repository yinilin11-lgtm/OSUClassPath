using System.ComponentModel.DataAnnotations;

namespace OSUClassPath.Models;

public enum RecommendedPlanItemType
{
    Course,
    GeneralEducation,
    Elective,
    Requirement
}

public class RecommendedPlanItem
{
    public int Id { get; set; }

    [Display(Name = "建議學期")]
    public int RecommendedPlanTermId { get; set; }

    public RecommendedPlanTerm? RecommendedPlanTerm { get; set; }

    [StringLength(30)]
    [Display(Name = "課程代碼")]
    public string? CourseCode { get; set; }

    [Required]
    [StringLength(160)]
    [Display(Name = "項目名稱")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "學分")]
    public int Credits { get; set; }

    [Display(Name = "項目類型")]
    public RecommendedPlanItemType ItemType { get; set; }

    [StringLength(300)]
    [Display(Name = "備註")]
    public string? Notes { get; set; }

    [Display(Name = "排序")]
    public int SortOrder { get; set; }
}
