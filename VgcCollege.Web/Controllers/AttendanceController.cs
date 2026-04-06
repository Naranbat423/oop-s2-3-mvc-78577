using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Models;
using VgcCollege.Web.Data;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Admin,Faculty")]
public class AttendanceController : Controller
{
    private readonly ApplicationDbContext _context;

    public AttendanceController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Attendance/SelectCourse
    public async Task<IActionResult> SelectCourse()
    {
        var userId = User.Identity?.Name;
        IQueryable<Course> coursesQuery = _context.Courses;

        if (!User.IsInRole("Admin"))
        {
            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == userId);
            if (faculty == null) return Forbid();

            var courseIds = await _context.FacultyCourses
                .Where(fc => fc.FacultyProfileId == faculty.Id)
                .Select(fc => fc.CourseId)
                .ToListAsync();
            coursesQuery = coursesQuery.Where(c => courseIds.Contains(c.Id));
        }

        var courses = await coursesQuery.ToListAsync();
        return View(courses);
    }

    // GET: Attendance/ByCourse/{courseId}
    public async Task<IActionResult> ByCourse(int courseId, int? weekNumber)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null) return NotFound();

        // Authorization: Admin or faculty assigned to this course
        var userId = User.Identity?.Name;
        if (!User.IsInRole("Admin"))
        {
            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == userId);
            if (faculty == null) return Forbid();

            var isAssigned = await _context.FacultyCourses
                .AnyAsync(fc => fc.FacultyProfileId == faculty.Id && fc.CourseId == courseId);
            if (!isAssigned) return Forbid();
        }

        var enrolments = await _context.CourseEnrolments
            .Include(e => e.Student)
            .Include(e => e.AttendanceRecords)
            .Where(e => e.CourseId == courseId && e.Status == "Active")
            .ToListAsync();

        // Determine max week number used so far (optional)
        var maxWeek = enrolments
            .SelectMany(e => e.AttendanceRecords)
            .Select(a => a.WeekNumber)
            .DefaultIfEmpty(0)
            .Max();

        ViewBag.CourseName = course.Name;
        ViewBag.CourseId = courseId;
        ViewBag.WeekNumber = weekNumber ?? (maxWeek + 1);
        ViewBag.AllWeeks = Enumerable.Range(1, 12).ToList(); // 12-week semester

        return View(enrolments);
    }

    // POST: Attendance/Save
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int courseId, int weekNumber, Dictionary<int, bool> attendance)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null) return NotFound();

        // Authorization check
        var userId = User.Identity?.Name;
        if (!User.IsInRole("Admin"))
        {
            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == userId);
            if (faculty == null) return Forbid();

            var isAssigned = await _context.FacultyCourses
                .AnyAsync(fc => fc.FacultyProfileId == faculty.Id && fc.CourseId == courseId);
            if (!isAssigned) return Forbid();
        }

        // Save or update attendance records
        foreach (var kvp in attendance)
        {
            var enrolmentId = kvp.Key;
            var present = kvp.Value;

            var existing = await _context.AttendanceRecords
                .FirstOrDefaultAsync(a => a.CourseEnrolmentId == enrolmentId && a.WeekNumber == weekNumber);
            if (existing != null)
            {
                existing.Present = present;
                _context.Update(existing);
            }
            else
            {
                _context.AttendanceRecords.Add(new AttendanceRecord
                {
                    CourseEnrolmentId = enrolmentId,
                    WeekNumber = weekNumber,
                    Present = present
                });
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("ByCourse", new { courseId, weekNumber });
    }
}