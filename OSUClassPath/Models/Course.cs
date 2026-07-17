using System.ComponentModel.DataAnnotations;

namespace OSUClassPath.Models;

public class Course
{
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    [Display(Name = "課程代碼")]
    public string CourseCode { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [Display(Name = "課程名稱")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "課程介紹")]
    public string Description { get; set; } = string.Empty;

    [Range(0, 20)]
    [Display(Name = "學分")]
    public int Credits { get; set; }

    [Display(Name = "先修條件")]
    public string PrerequisiteText { get; set; } = string.Empty;

    [Display(Name = "官方來源")]
    [Url]
    public string SourceUrl { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "最後確認日期")]
    public DateTime LastVerified { get; set; } = DateTime.Today;
}