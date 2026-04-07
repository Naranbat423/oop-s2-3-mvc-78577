using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Models;
using VgcCollege.Web.Data;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Admin,Faculty")]
public class AttendanceController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public AttendanceController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> SelectCourse()
    {
        IQueryable<Course> coursesQuery = _context.Courses.Include(c => c.Branch);

        if (!User.IsInRole("Admin"))
        {
            var userId = _userManager.GetUserId(User);
            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == userId);
            if (faculty == null) return Forbid();

            var courseIds = await _context.FacultyCourses
                .Where(fc => fc.FacultyProfileId == faculty.Id)
                .Select(fc => fc.CourseId)
                .ToListAsync();

            coursesQuery = coursesQuery.Where(c => courseIds.Contains(c.Id));
        }

        var courses = await coursesQuery.OrderBy(c => c.Name).ToListAsync();
        return View(courses);
    }

    public async Task<IActionResult> ByCourse(int courseId, int? weekNumber)
    {
        var course = await _context.Courses
            .Include(c => c.Branch)
            .FirstOrDefaultAsync(c => c.Id == courseId);
        if (course == null) return NotFound();

        if (!User.IsInRole("Admin"))
        {
            var userId = _userManager.GetUserId(User);
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
            .OrderBy(e => e.Student.Name)
            .ToListAsync();

        var maxWeek = enrolments
            .SelectMany(e => e.AttendanceRecords)
            .Select(a => a.WeekNumber)
            .DefaultIfEmpty(0)
            .Max();

        ViewBag.CourseName = course.Name;
        ViewBag.CourseId = courseId;
        ViewBag.WeekNumber = weekNumber ?? (maxWeek + 1);
        ViewBag.AllWeeks = Enumerable.Range(1, 12).ToList();

        return View(enrolments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(
        int courseId,
        int weekNumber,
        List<int> presentIds,
        List<int> allEnrolmentIds)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null) return NotFound();

        if (!User.IsInRole("Admin"))
        {
            var userId = _userManager.GetUserId(User);
            var faculty = await _context.FacultyProfiles
                .FirstOrDefaultAsync(f => f.IdentityUserId == userId);
            if (faculty == null) return Forbid();

            var isAssigned = await _context.FacultyCourses
                .AnyAsync(fc => fc.FacultyProfileId == faculty.Id && fc.CourseId == courseId);
            if (!isAssigned) return Forbid();
        }

        foreach (var enrolmentId in allEnrolmentIds)
        {
            var present = presentIds.Contains(enrolmentId);

            var existing = await _context.AttendanceRecords
                .FirstOrDefaultAsync(a => a.CourseEnrolmentId == enrolmentId
                                       && a.WeekNumber == weekNumber);
            if (existing != null)
            {
                existing.Present = present;
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
        TempData["Message"] = $"Attendance saved for Week {weekNumber}.";
        return RedirectToAction("ByCourse", new { courseId, weekNumber });
    }
}