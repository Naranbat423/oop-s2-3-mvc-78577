using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Models;

namespace VgcCollege.Web.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>

{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Branch> Branches { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<StudentProfile> StudentProfiles { get; set; }
    public DbSet<FacultyProfile> FacultyProfiles { get; set; }
    public DbSet<FacultyCourse> FacultyCourses { get; set; }
    public DbSet<CourseEnrolment> CourseEnrolments { get; set; }
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<AssignmentResult> AssignmentResults { get; set; }
    public DbSet<Exam> Exams { get; set; }
    public DbSet<ExamResult> ExamResults { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<FacultyCourse>().HasKey(fc => new { fc.FacultyProfileId, fc.CourseId });
    }
}