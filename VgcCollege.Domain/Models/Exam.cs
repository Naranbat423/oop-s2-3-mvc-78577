using System.ComponentModel.DataAnnotations;

namespace VgcCollege.Domain.Models;

public class Exam
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Course")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Exam title is required.")]
    [MaxLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
    public string Title { get; set; } = "";

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Exam Date")]
    public DateTime Date { get; set; }

    [Required]
    [Range(1, 1000, ErrorMessage = "Max score must be between 1 and 1000.")]
    [Display(Name = "Max Score")]
    public int MaxScore { get; set; }

    [Display(Name = "Results Released")]
    public bool ResultsReleased { get; set; } = false;

    // Navigation
    public Course Course { get; set; } = null!;
    public ICollection<ExamResult> Results { get; set; } = new List<ExamResult>();
}