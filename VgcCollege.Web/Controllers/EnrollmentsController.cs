using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Models;
using VgcCollege.Web.Data;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Admin")]
public class EnrollmentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public EnrollmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var enrolments = await _context.CourseEnrolments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .OrderBy(e => e.Student.Name)
            .ToListAsync();
        return View(enrolments);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var enrolment = await _context.CourseEnrolments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (enrolment == null) return NotFound();
        return View(enrolment);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("StudentProfileId,CourseId")] CourseEnrolment enrolment)
    {
        var errors = new List<string>();

        if (enrolment.StudentProfileId == 0)
            errors.Add("Please select a student.");
        if (enrolment.CourseId == 0)
            errors.Add("Please select a course.");

        if (enrolment.StudentProfileId > 0 && enrolment.CourseId > 0)
        {
            var duplicate = await _context.CourseEnrolments
                .AnyAsync(e => e.StudentProfileId == enrolment.StudentProfileId
                            && e.CourseId == enrolment.CourseId);
            if (duplicate)
                errors.Add("This student is already enrolled in this course.");
        }

        if (errors.Any())
        {
            foreach (var err in errors)
                ModelState.AddModelError("", err);
            await PopulateDropdowns();
            return View(enrolment);
        }

        enrolment.EnrolDate = DateTime.Today;
        enrolment.Status = "Active";
        _context.CourseEnrolments.Add(enrolment);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var enrolment = await _context.CourseEnrolments.FindAsync(id);
        if (enrolment == null) return NotFound();
        await PopulateDropdowns(enrolment.StudentProfileId, enrolment.CourseId);
        return View(enrolment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,StudentProfileId,CourseId,EnrolDate,Status")] CourseEnrolment enrolment)
    {
        if (id != enrolment.Id) return NotFound();

        ModelState.Remove("Student");
        ModelState.Remove("Course");
        ModelState.Remove("AttendanceRecords");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(enrolment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.CourseEnrolments.Any(e => e.Id == enrolment.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        await PopulateDropdowns(enrolment.StudentProfileId, enrolment.CourseId);
        return View(enrolment);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var enrolment = await _context.CourseEnrolments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (enrolment == null) return NotFound();
        return View(enrolment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var enrolment = await _context.CourseEnrolments.FindAsync(id);
        if (enrolment != null)
        {
            _context.CourseEnrolments.Remove(enrolment);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdowns(int? selectedStudentId = null, int? selectedCourseId = null)
    {
        ViewBag.StudentId = new SelectList(
            await _context.StudentProfiles.OrderBy(s => s.Name).ToListAsync(),
            "Id", "Name", selectedStudentId);
        ViewBag.CourseId = new SelectList(
            await _context.Courses.OrderBy(c => c.Name).ToListAsync(),
            "Id", "Name", selectedCourseId);
    }
}