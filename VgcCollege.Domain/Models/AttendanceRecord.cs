using System.ComponentModel.DataAnnotations;

namespace VgcCollege.Domain.Models;

public class AttendanceRecord
{
    public int Id { get; set; }

    [Required]
    public int CourseEnrolmentId { get; set; }

    [Required]
    [Range(1, 52, ErrorMessage = "Week number must be between 1 and 52.")]
    [Display(Name = "Week Number")]
    public int WeekNumber { get; set; }

    [Display(Name = "Present")]
    public bool Present { get; set; }

    // Navigation
    public CourseEnrolment Enrolment { get; set; } = null!;
}