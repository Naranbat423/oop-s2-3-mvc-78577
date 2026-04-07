using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace VgcCollege.Domain.Models;

public class FacultyProfile
{
    public int Id { get; set; }

    public string? IdentityUserId { get; set; }

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

    // Navigation
    public IdentityUser IdentityUser { get; set; } = null!;
    public ICollection<FacultyCourse> FacultyCourses { get; set; } = new List<FacultyCourse>();
}