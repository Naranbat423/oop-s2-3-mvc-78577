using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Models;
using VgcCollege.Web.Data;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Admin")]
public class StaffAssignmentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public StaffAssignmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: StaffAssignments
    public async Task<IActionResult> Index()
    {
        var assignments = await _context.FacultyCourses
            .Include(fc => fc.Faculty)
            .Include(fc => fc.Course)
                .ThenInclude(c => c.Branch)
            .OrderBy(fc => fc.Faculty.Name)
            .ToListAsync();
        return View(assignments);
    }

    // GET: StaffAssignments/Create
    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View();
    }

    // POST: StaffAssignments/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("FacultyProfileId,CourseId,IsTutor")] FacultyCourse assignment)
    {
        // Remove navigation properties from ModelState — they are not posted by the form
        ModelState.Remove("Faculty");
        ModelState.Remove("Course");

        if (ModelState.IsValid)
        {
            var exists = await _context.FacultyCourses
                .AnyAsync(fc => fc.FacultyProfileId == assignment.FacultyProfileId
                             && fc.CourseId == assignment.CourseId);
            if (exists)
            {
                ModelState.AddModelError("", "This faculty member is already assigned to this course.");
                await PopulateDropdowns();
                return View(assignment);
            }

            _context.Add(assignment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        await PopulateDropdowns();
        return View(assignment);
    }

    // GET: StaffAssignments/Delete?facultyId=1&courseId=2
    public async Task<IActionResult> Delete(int? facultyId, int? courseId)
    {
        if (facultyId == null || courseId == null) return NotFound();

        var assignment = await _context.FacultyCourses
            .Include(fc => fc.Faculty)
            .Include(fc => fc.Course)
                .ThenInclude(c => c.Branch)
            .FirstOrDefaultAsync(fc => fc.FacultyProfileId == facultyId
                                    && fc.CourseId == courseId);
        if (assignment == null) return NotFound();

        return View(assignment);
    }

    // POST: StaffAssignments/Delete
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int facultyId, int courseId)
    {
        var assignment = await _context.FacultyCourses
            .FirstOrDefaultAsync(fc => fc.FacultyProfileId == facultyId
                                    && fc.CourseId == courseId);
        if (assignment != null)
        {
            _context.FacultyCourses.Remove(assignment);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdowns(int? selectedFaculty = null, int? selectedCourse = null)
    {
        ViewBag.FacultyId = new SelectList(
            await _context.FacultyProfiles.OrderBy(f => f.Name).ToListAsync(),
            "Id", "Name", selectedFaculty);
        ViewBag.CourseId = new SelectList(
            await _context.Courses.OrderBy(c => c.Name).ToListAsync(),
            "Id", "Name", selectedCourse);
    }
}