using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Web.Data;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public StudentController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<VgcCollege.Domain.Models.StudentProfile?> GetCurrentStudentAsync()
    {
        var userId = _userManager.GetUserId(User);
        return await _context.StudentProfiles
            .FirstOrDefaultAsync(s => s.IdentityUserId == userId);
    }

    // GET: Student/MyProfile
    public async Task<IActionResult> MyProfile()
    {
        var student = await GetCurrentStudentAsync();
        if (student == null)
            return Content("No student profile found for your account.");
        return View(student);
    }

    // GET: Student/MyGrades
    public async Task<IActionResult> MyGrades()
    {
        var student = await GetCurrentStudentAsync();
        if (student == null) return NotFound();

        var assignmentResults = await _context.AssignmentResults
            .Include(r => r.Assignment)
                .ThenInclude(a => a.Course)
            .Where(r => r.StudentProfileId == student.Id)
            .OrderBy(r => r.Assignment.Course.Name)
            .ThenBy(r => r.Assignment.DueDate)
            .ToListAsync();

        // Only show released exam results — server-side filter
        var examResults = await _context.ExamResults
            .Include(r => r.Exam)
                .ThenInclude(e => e.Course)
            .Where(r => r.StudentProfileId == student.Id
                     && r.Exam.ResultsReleased == true)
            .OrderBy(r => r.Exam.Course.Name)
            .ThenBy(r => r.Exam.Date)
            .ToListAsync();

        ViewBag.AssignmentResults = assignmentResults;
        ViewBag.ExamResults = examResults;
        ViewBag.StudentName = student.Name;

        return View();
    }

    // GET: Student/MyAttendance
    public async Task<IActionResult> MyAttendance()
    {
        var student = await GetCurrentStudentAsync();
        if (student == null) return NotFound();

        var enrolments = await _context.CourseEnrolments
            .Include(e => e.Course)
            .Include(e => e.AttendanceRecords)
            .Where(e => e.StudentProfileId == student.Id)
            .OrderBy(e => e.Course.Name)
            .ToListAsync();

        ViewBag.StudentName = student.Name;
        return View(enrolments);
    }
}