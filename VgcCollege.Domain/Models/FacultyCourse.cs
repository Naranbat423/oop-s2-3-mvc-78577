using System.ComponentModel.DataAnnotations;

namespace VgcCollege.Domain.Models;

public class FacultyCourse
{
    [Required]
    public int FacultyProfileId { get; set; }

    [Required]
    public int CourseId { get; set; }

    // Tutors can access student contact details; non-tutors cannot
    [Display(Name = "Is Tutor")]
    public bool IsTutor { get; set; } = false;

    // Navigation
    public FacultyProfile Faculty { get; set; } = null!;
    public Course Course { get; set; } = null!;
}