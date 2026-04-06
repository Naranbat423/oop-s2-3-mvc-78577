namespace VgcCollege.Domain.Models;
public class CourseEnrolment
{
    public int Id { get; set; }
    public int StudentProfileId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrolDate { get; set; }
    public string Status { get; set; } = "Active";
    public StudentProfile Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}