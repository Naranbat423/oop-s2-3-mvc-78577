using System.ComponentModel.DataAnnotations;

namespace VgcCollege.Domain.Models;

public class Assignment
{
    public int Id { get; set; }

    [Required]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
    [Display(Name = "Assignment Title")]
    public string Title { get; set; } = "";

    [Required]
    [Range(1, 1000, ErrorMessage = "Max score must be between 1 and 1000.")]
    [Display(Name = "Max Score")]
    public int MaxScore { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Due Date")]
    public DateTime DueDate { get; set; }

    // Navigation
    public Course Course { get; set; } = null!;
    public ICollection<AssignmentResult> Results { get; set; } = new List<AssignmentResult>();
}