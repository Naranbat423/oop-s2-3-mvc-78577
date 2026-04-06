using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    // GET: Exams (list all exams with release status)
    public async Task<IActionResult> Index()
    {
        var exams = await _context.Exams
            .Include(e => e.Course)
            .ToListAsync();
        return View(exams);
    }

    // GET: Exams/ReleaseResults (list exams to release)
    public async Task<IActionResult> ReleaseResults()
    {
        var exams = await _context.Exams
            .Include(e => e.Course)
            .ToListAsync();
        return View(exams);
    }

    // POST: Exams/ReleaseResults/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReleaseResults(int id)
    {
        var exam = await _context.Exams.FindAsync(id);
        if (exam == null) return NotFound();

        exam.ResultsReleased = !exam.ResultsReleased; // toggle
        await _context.SaveChangesAsync();

        // Redirect back to the ReleaseResults list
        return RedirectToAction(nameof(ReleaseResults));
    }
}