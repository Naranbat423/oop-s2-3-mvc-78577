using System.ComponentModel.DataAnnotations;

namespace VgcCollege.Domain.Models;

public class CourseEnrolment
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Student")]
    public int StudentProfileId { get; set; }

    [Required]
    [Display(Name = "Course")]
    public int CourseId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Enrolment Date")]
    public DateTime EnrolDate { get; set; } = DateTime.Today;

    [Required]
    [MaxLength(20)]
    // Valid values: Active, Completed, Dropped
    public string Status { get; set; } = "Active";

    // Navigation
    public StudentProfile Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}