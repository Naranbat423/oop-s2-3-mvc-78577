using System.ComponentModel.DataAnnotations;

namespace VgcCollege.Domain.Models;

public class AssignmentResult
{
    public int Id { get; set; }

    [Required]
    public int AssignmentId { get; set; }

    [Required]
    public int StudentProfileId { get; set; }

    [Required]
    [Range(0, 1000, ErrorMessage = "Score must be between 0 and the max score.")]
    public int Score { get; set; }

    [MaxLength(1000, ErrorMessage = "Feedback cannot exceed 1000 characters.")]
    [DataType(DataType.MultilineText)]
    public string Feedback { get; set; } = "";

    // Navigation
    public Assignment Assignment { get; set; } = null!;
    public StudentProfile Student { get; set; } = null!;
}