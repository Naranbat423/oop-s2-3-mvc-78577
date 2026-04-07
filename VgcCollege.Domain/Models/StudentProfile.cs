using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace VgcCollege.Domain.Models;

public class StudentProfile
{
    public int Id { get; set; }

    [Required]
    public string IdentityUserId { get; set; } = "";

    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [MaxLength(150)]
    public string Email { get; set; } = "";

    [Phone(ErrorMessage = "Enter a valid phone number.")]
    [MaxLength(20)]
    public string Phone { get; set; } = "";

    [MaxLength(200)]
    public string Address { get; set; } = "";

    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime? DOB { get; set; }

    [Required(ErrorMessage = "Student number is required.")]
    [MaxLength(20)]
    [Display(Name = "Student Number")]
    public string StudentNumber { get; set; } = "";

    // Navigation
    public IdentityUser IdentityUser { get; set; } = null!;
    public ICollection<CourseEnrolment> Enrolments { get; set; } = new List<CourseEnrolment>();
    public ICollection<AssignmentResult> AssignmentResults { get; set; } = new List<AssignmentResult>();
    public ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
}