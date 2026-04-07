using System.ComponentModel.DataAnnotations;

namespace VgcCollege.Domain.Models;

public class Branch
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Branch name is required.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Address is required.")]
    [MaxLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
    public string Address { get; set; } = "";

    // Navigation
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}