using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Faculty")]
public class FacultyController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public FacultyController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Faculty/MyStudents
    // Shows all students enrolled in courses this faculty member teaches
    public async Task<IActionResult> MyStudents()
    {
        // CORRECT: use GetUserId, not Identity.Name — Name returns email, not the GUID
        var userId = _userManager.GetUserId(User);

        var faculty = await _context.FacultyProfiles
            .FirstOrDefaultAsync(f => f.IdentityUserId == userId);

        if (faculty == null) return Forbid();

        // Get the course IDs this faculty teaches
        var courseIds = await _context.FacultyCourses
            .Where(fc => fc.FacultyProfileId == faculty.Id)
            .Select(fc => fc.CourseId)
            .ToListAsync();

        // Get all active enrolments for those courses, with student + course info
        var enrolments = await _context.CourseEnrolments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Where(e => courseIds.Contains(e.CourseId) && e.Status == "Active")
            .OrderBy(e => e.Course.Name)
            .ThenBy(e => e.Student.Name)
            .ToListAsync();

        // Determine which courses this faculty tutors (for contact detail access)
        var tutorCourseIds = await _context.FacultyCourses
            .Where(fc => fc.FacultyProfileId == faculty.Id && fc.IsTutor)
            .Select(fc => fc.CourseId)
            .ToListAsync();

        ViewBag.TutorCourseIds = tutorCourseIds;
        ViewBag.FacultyName = faculty.Name;

        return View(enrolments);
    }

    // GET: Faculty/StudentContact/5
    // Only tutors can see contact details — enforced server-side
    public async Task<IActionResult> StudentContact(int studentId, int courseId)
    {
        var userId = _userManager.GetUserId(User);

        var faculty = await _context.FacultyProfiles
            .FirstOrDefaultAsync(f => f.IdentityUserId == userId);

        if (faculty == null) return Forbid();

        // Server-side check: this faculty must be a TUTOR for this specific course
        var isTutor = await _context.FacultyCourses
            .AnyAsync(fc => fc.FacultyProfileId == faculty.Id
                         && fc.CourseId == courseId
                         && fc.IsTutor);

        if (!isTutor) return Forbid();

        // Confirm student is actually enrolled in that course
        var enrolment = await _context.CourseEnrolments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.StudentProfileId == studentId
                                   && e.CourseId == courseId
                                   && e.Status == "Active");

        if (enrolment == null) return NotFound();

        return View(enrolment.Student);
    }

    // GET: Faculty/Gradebook
    // Redirects to GradebookController/SelectCourse for cleaner separation
    public IActionResult Gradebook() => RedirectToAction("SelectCourse", "Gradebook");
}