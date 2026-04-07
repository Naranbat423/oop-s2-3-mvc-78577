using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Models;
using VgcCollege.Web.Data;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Admin")]
public class ExamsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ExamsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Exams
    public async Task<IActionResult> Index()
    {
        var exams = await _context.Exams
            .Include(e => e.Course)
                .ThenInclude(c => c.Branch)
            .OrderBy(e => e.Course.Name)
            .ThenBy(e => e.Date)
            .ToListAsync();
        return View(exams);
    }

    // GET: Exams/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Courses = new SelectList(
            await _context.Courses.Include(c => c.Branch).OrderBy(c => c.Name).ToListAsync(),
            "Id", "Name");
        return View();
    }

    // POST: Exams/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CourseId,Title,Date,MaxScore,ResultsReleased")] Exam exam)
    {
        if (ModelState.IsValid)
        {
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Courses = new SelectList(
            await _context.Courses.Include(c => c.Branch).OrderBy(c => c.Name).ToListAsync(),
            "Id", "Name", exam.CourseId);
        return View(exam);
    }

    // GET: Exams/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var exam = await _context.Exams.FindAsync(id);
        if (exam == null) return NotFound();

        ViewBag.Courses = new SelectList(
            await _context.Courses.Include(c => c.Branch).OrderBy(c => c.Name).ToListAsync(),
            "Id", "Name", exam.CourseId);
        return View(exam);
    }

    // POST: Exams/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,CourseId,Title,Date,MaxScore,ResultsReleased")] Exam exam)
    {
        if (id != exam.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(exam);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Exams.Any(e => e.Id == exam.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Courses = new SelectList(
            await _context.Courses.Include(c => c.Branch).OrderBy(c => c.Name).ToListAsync(),
            "Id", "Name", exam.CourseId);
        return View(exam);
    }

    // GET: Exams/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var exam = await _context.Exams
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (exam == null) return NotFound();

        return View(exam);
    }

    // POST: Exams/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var exam = await _context.Exams.FindAsync(id);
        if (exam != null)
        {
            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: Exams/ReleaseResults — lists all exams with toggle buttons
    public async Task<IActionResult> ReleaseResults()
    {
        var exams = await _context.Exams
            .Include(e => e.Course)
                .ThenInclude(c => c.Branch)
            .OrderBy(e => e.Course.Name)
            .ThenBy(e => e.Date)
            .ToListAsync();
        return View(exams);
    }

    // POST: Exams/ReleaseResults/5 — toggles ResultsReleased
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReleaseResults(int id)
    {
        var exam = await _context.Exams.FindAsync(id);
        if (exam == null) return NotFound();

        exam.ResultsReleased = !exam.ResultsReleased;
        await _context.SaveChangesAsync();

        TempData["Message"] = exam.ResultsReleased
            ? $"Results for \"{exam.Title}\" are now visible to students."
            : $"Results for \"{exam.Title}\" are now hidden (provisional).";

        return RedirectToAction(nameof(ReleaseResults));
    }

    // GET: Exams/Results/5 — Admin views all student results for an exam
    public async Task<IActionResult> Results(int? id)
    {
        if (id == null) return NotFound();

        var exam = await _context.Exams
            .Include(e => e.Course)
            .Include(e => e.Results)
                .ThenInclude(r => r.Student)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (exam == null) return NotFound();

        return View(exam);
    }
}