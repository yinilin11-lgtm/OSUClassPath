using System.ComponentModel.DataAnnotations;

namespace OSUClassPath.Models;

public enum CourseStatus
{
    [Display(Name = "計畫修讀")]
    Planned,

    [Display(Name = "正在修讀")]
    InProgress,

    [Display(Name = "已完成")]
    Completed,

    [Display(Name = "轉學分")]
    Transferred
}

public class StudentCourse
{
    public int Id { get; set; }

    [Display(Name = "學生")]
    public int StudentId { get; set; }

    public Student? Student { get; set; }

    [Display(Name = "課程")]
    public int CourseId { get; set; }

    public Course? Course { get; set; }

    [Display(Name = "課程狀態")]
    public CourseStatus Status { get; set; }

    [StringLength(30)]
    [Display(Name = "修讀學期")]
    public string? Term { get; set; }

    [StringLength(10)]
    [Display(Name = "成績")]
    public string? Grade { get; set; }
}