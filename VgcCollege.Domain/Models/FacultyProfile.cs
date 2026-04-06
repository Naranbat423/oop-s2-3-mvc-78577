using Microsoft.AspNetCore.Identity;
namespace VgcCollege.Domain.Models;
public class FacultyProfile
{
    public int Id { get; set; }
    public string? IdentityUserId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public IdentityUser IdentityUser { get; set; } = null!;
    public ICollection<FacultyCourse> FacultyCourses { get; set; } = new List<FacultyCourse>();
}