using System.ComponentModel.DataAnnotations;

namespace VgcCollege.Domain.Models;

public class ExamResult
{
    public int Id { get; set; }

    [Required]
    public int ExamId { get; set; }

    [Required]
    public int StudentProfileId { get; set; }

    [Required]
    [Range(0, 1000, ErrorMessage = "Score must be between 0 and the max score.")]
    public int Score { get; set; }

    [MaxLength(10, ErrorMessage = "Grade cannot exceed 10 characters.")]
    public string Grade { get; set; } = "";

    // Navigation
    public Exam Exam { get; set; } = null!;
    public StudentProfile Student { get; set; } = null!;
}