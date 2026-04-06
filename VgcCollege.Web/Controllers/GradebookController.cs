using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Models;
using VgcCollege.Web.Data;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Faculty")]
public class GradebookController : Controller
{
    private readonly ApplicationDbContext _context;

    public GradebookController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Gradebook/SelectCourse
    public async Task<IActionResult> SelectCourse()
    {
        var userId = User.Identity?.Name;
        var faculty = await _context.FacultyProfiles
            .FirstOrDefaultAsync(f => f.IdentityUserId == userId);
        if (faculty == null) return Forbid();

        var courseIds = await _context.FacultyCourses
            .Where(fc => fc.FacultyProfileId == faculty.Id)
            .Select(fc => fc.CourseId)
            .ToListAsync();

        var courses = await _context.Courses
            .Where(c => courseIds.Contains(c.Id))
            .ToListAsync();

        return View(courses);
    }

    // GET: Gradebook/Assignments/{courseId}
    public async Task<IActionResult> Assignments(int courseId)
    {
        var assignments = await _context.Assignments
            .Where(a => a.CourseId == courseId)
            .ToListAsync();

        ViewBag.CourseId = courseId;
        return View(assignments);
    }

    // GET: Gradebook/Results/{assignmentId}
    public async Task<IActionResult> Results(int assignmentId)
    {
        var assignment = await _context.Assignments
            .Include(a => a.Course)
            .FirstOrDefaultAsync(a => a.Id == assignmentId);
        if (assignment == null) return NotFound();

        // Get all enrolments for the course (active students)
        var enrolments = await _context.CourseEnrolments
            .Include(e => e.Student)
            .Where(e => e.CourseId == assignment.CourseId && e.Status == "Active")
            .ToListAsync();

        // Get existing results
        var results = await _context.AssignmentResults
            .Where(r => r.AssignmentId == assignmentId)
            .ToDictionaryAsync(r => r.StudentProfileId, r => r);

        ViewBag.Assignment = assignment;
        return View(Tuple.Create(enrolments, results));
    }

    // POST: Gradebook/SaveResults
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveResults(int assignmentId, Dictionary<int, int> scores, Dictionary<int, string> feedbacks)
    {
        var assignment = await _context.Assignments.FindAsync(assignmentId);
        if (assignment == null) return NotFound();

        foreach (var kvp in scores)
        {
            var studentId = kvp.Key;
            var score = kvp.Value;
            var feedback = feedbacks.ContainsKey(studentId) ? feedbacks[studentId] : "";

            var existing = await _context.AssignmentResults
                .FirstOrDefaultAsync(r => r.AssignmentId == assignmentId && r.StudentProfileId == studentId);
            if (existing != null)
            {
                existing.Score = score;
                existing.Feedback = feedback;
                _context.Update(existing);
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