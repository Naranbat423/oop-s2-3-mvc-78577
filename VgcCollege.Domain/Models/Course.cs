using System.ComponentModel.DataAnnotations;

namespace VgcCollege.Domain.Models;

public class Course
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Course name is required.")]
    [MaxLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
    public string Name { get; set; } = "";

    [Required]
    [Display(Name = "Branch")]
    public int BranchId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "End Date")]
    public DateTime EndDate { get; set; }

    // Navigation
    public Branch Branch { get; set; } = null!;
    public ICollection<CourseEnrolment> Enrolments { get; set; } = new List<CourseEnrolment>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
    public ICollection<FacultyCourse> FacultyCourses { get; set; } = new List<FacultyCourse>();
}