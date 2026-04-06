namespace VgcCollege.Domain.Models;
public class FacultyCourse
{
    public int FacultyProfileId { get; set; }
    public int CourseId { get; set; }
    public FacultyProfile Faculty { get; set; } = null!;
    public Course Course { get; set; } = null!;
}