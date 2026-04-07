using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Models;
using VgcCollege.Web.Data;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Faculty")]
public class GradebookController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public GradebookController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Shared helper: gets the faculty profile for the logged-in user.
    // Returns null if not found — callers must handle this.
    private async Task<VgcCollege.Domain.Models.FacultyProfile?> GetCurrentFacultyAsync()
    {
        // FIXED: GetUserId returns the GUID — Identity.Name returns email which won't match
        var userId = _userManager.GetUserId(User);
        return await _context.FacultyProfiles
            .FirstOrDefaultAsync(f => f.IdentityUserId == userId);
    }

    // GET: Gradebook/SelectCourse
    public async Task<IActionResult> SelectCourse()
    {
        var faculty = await GetCurrentFacultyAsync();
        if (faculty == null) return Forbid();

        var courseIds = await _context.FacultyCourses
            .Where(fc => fc.FacultyProfileId == faculty.Id)
            .Select(fc => fc.CourseId)
            .ToListAsync();

        var courses = await _context.Courses
            .Include(c => c.Branch)
            .Where(c => courseIds.Contains(c.Id))
            .OrderBy(c => c.Name)
            .ToListAsync();

        return View(courses);
    }

    // GET: Gradebook/Assignments/{courseId}
    public async Task<IActionResult> Assignments(int courseId)
    {
        var faculty = await GetCurrentFacultyAsync();
        if (faculty == null) return Forbid();

        // Server-side: confirm this faculty teaches this course
        var teaches = await _context.FacultyCourses
            .AnyAsync(fc => fc.FacultyProfileId == faculty.Id && fc.CourseId == courseId);
        if (!teaches) return Forbid();

        var course = await _context.Courses
            .Include(c => c.Branch)
            .FirstOrDefaultAsync(c => c.Id == courseId);
        if (course == null) return NotFound();

        var assignments = await _context.Assignments
            .Where(a => a.CourseId == courseId)
            .OrderBy(a => a.DueDate)
            .ToListAsync();

        ViewBag.Course = course;
        return View(assignments);
    }

    // GET: Gradebook/Results/{assignmentId}
    public async Task<IActionResult> Results(int assignmentId)
    {
        var faculty = await GetCurrentFacultyAsync();
        if (faculty == null) return Forbid();

        var assignment = await _context.Assignments
            .Include(a => a.Course)
            .FirstOrDefaultAsync(a => a.Id == assignmentId);
        if (assignment == null) return NotFound();

        // Server-side: confirm faculty teaches the course this assignment belongs to
        var teaches = await _context.FacultyCourses
            .AnyAsync(fc => fc.FacultyProfileId == faculty.Id
                         && fc.CourseId == assignment.CourseId);
        if (!teaches) return Forbid();

        // Only active students enrolled in this course
        var enrolments = await _context.CourseEnrolments
            .Include(e => e.Student)
            .Where(e => e.CourseId == assignment.CourseId && e.Status == "Active")
            .OrderBy(e => e.Student.Name)
            .ToListAsync();

        // Existing results keyed by studentId for easy lookup in view
        var results = await _context.AssignmentResults
            .Where(r => r.AssignmentId == assignmentId)
            .ToDictionaryAsync(r => r.StudentProfileId, r => r);

        ViewBag.Assignment = assignment;
        return View(Tuple.Create(enrolments, results));
    }

    // POST: Gradebook/SaveResults
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveResults(
        int assignmentId,
        Dictionary<int, int> scores,
        Dictionary<int, string> feedbacks)
    {
        var faculty = await GetCurrentFacultyAsync();
        if (faculty == null) return Forbid();

        var assignment = await _context.Assignments.FindAsync(assignmentId);
        if (assignment == null) return NotFound();

        // Server-side: faculty must teach this course
        var teaches = await _context.FacultyCourses
            .AnyAsync(fc => fc.FacultyProfileId == faculty.Id
                         && fc.CourseId == assignment.CourseId);
        if (!teaches) return Forbid();

        foreach (var kvp in scores)
        {
            var studentId = kvp.Key;
            var score = kvp.Value;

            // Clamp score so it never exceeds MaxScore (server-side enforcement)
            score = Math.Max(0, Math.Min(score, assignment.MaxScore));

            var feedback = feedbacks.GetValueOrDefault(studentId, "");

            var existing = await _context.AssignmentResults
                .FirstOrDefaultAsync(r => r.AssignmentId == assignmentId
                                       && r.StudentProfileId == studentId);
            if (existing != null)
            {
                existing.Score = score;
                existing.Feedback = feedback;
            }
            else
            {
                _context.AssignmentResults.Add(new AssignmentResult
                {
                    AssignmentId = assignmentId,
                    StudentProfileId = studentId,
                    Score = score,
                    Feedback = feedback
                });
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Results", new { assignmentId });
    }
}