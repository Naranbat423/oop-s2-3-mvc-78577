using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Models;
using VgcCollege.Web.Data;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Admin")]
public class StudentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public StudentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var students = await _context.StudentProfiles.OrderBy(s => s.Name).ToListAsync();
        return View(students);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var student = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.Id == id);
        if (student == null) return NotFound();
        return View(student);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("StudentNumber,Name,Email,Phone,Address,DOB")] StudentProfile student)
    {
        ModelState.Remove("IdentityUser");
        ModelState.Remove("IdentityUserId");
        ModelState.Remove("Enrolments");
        ModelState.Remove("AssignmentResults");
        ModelState.Remove("ExamResults");

        if (ModelState.IsValid)
        {
            // Set to null — admin-created profiles have no login account yet
            // SQLite will store NULL which does not violate the FK constraint
            student.IdentityUserId = null!;
            _context.Add(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(student);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var student = await _context.StudentProfiles.FindAsync(id);
        if (student == null) return NotFound();
        return View(student);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,IdentityUserId,StudentNumber,Name,Email,Phone,Address,DOB")] StudentProfile student)
    {
        if (id != student.Id) return NotFound();

        ModelState.Remove("IdentityUser");
        ModelState.Remove("Enrolments");
        ModelState.Remove("AssignmentResults");
        ModelState.Remove("ExamResults");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(student);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.StudentProfiles.Any(s => s.Id == student.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(student);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var student = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.Id == id);
        if (student == null) return NotFound();
        return View(student);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var student = await _context.StudentProfiles.FindAsync(id);
        if (student != null)
        {
            _context.StudentProfiles.Remove(student);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}