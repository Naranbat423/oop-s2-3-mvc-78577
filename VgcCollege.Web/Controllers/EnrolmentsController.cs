using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Models;
using VgcCollege.Web.Data;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Admin")]
public class EnrolmentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public EnrolmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Enrolments
    public async Task<IActionResult> Index()
    {
        var enrolments = await _context.CourseEnrolments
            .Include(e => e.Student)
            .Include(e => e.Course)
            .ToListAsync();
        return View(enrolments);
    }

    // GET: Enrolments/Details/5
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

    // GET: Enrolments/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.StudentId = new SelectList(await _context.StudentProfiles.ToListAsync(), "Id", "Name");
        ViewBag.CourseId = new SelectList(await _context.Courses.ToListAsync(), "Id", "Name");
        return View();
    }

    // POST: Enrolments/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("StudentProfileId,CourseId,EnrolDate,Status")] CourseEnrolment enrolment)
    {
        if (ModelState.IsValid)
        {
            // Check for duplicate enrolment
            var exists = await _context.CourseEnrolments
                .AnyAsync(e => e.StudentProfileId == enrolment.StudentProfileId && e.CourseId == enrolment.CourseId);
            if (exists)
            {
                ModelState.AddModelError("", "This student is already enrolled in this course.");
                await PopulateDropdowns();
                return View(enrolment);
            }

            enrolment.EnrolDate = DateTime.Today;
            enrolment.Status = "Active";
            _context.Add(enrolment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        await PopulateDropdowns();
        return View(enrolment);
    }

    // GET: Enrolments/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var enrolment = await _context.CourseEnrolments.FindAsync(id);
        if (enrolment == null) return NotFound();

        await PopulateDropdowns(enrolment.StudentProfileId, enrolment.CourseId);
        return View(enrolment);
    }

    // POST: Enrolments/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,StudentProfileId,CourseId,EnrolDate,Status")] CourseEnrolment enrolment)
    {
        if (id != enrolment.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(enrolment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EnrolmentExists(enrolment.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        await PopulateDropdowns(enrolment.StudentProfileId, enrolment.CourseId);
        return View(enrolment);
    }

    // GET: Enrolments/Delete/5
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

    // POST: Enrolments/Delete/5
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
        ViewBag.StudentId = new SelectList(await _context.StudentProfiles.ToListAsync(), "Id", "Name", selectedStudentId);
        ViewBag.CourseId = new SelectList(await _context.Courses.ToListAsync(), "Id", "Name", selectedCourseId);
    }

    private bool EnrolmentExists(int id)
    {
        return _context.CourseEnrolments.Any(e => e.Id == id);
    }
}