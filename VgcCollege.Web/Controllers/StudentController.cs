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

    // GET: Students
    public async Task<IActionResult> Index()
    {
        var students = await _context.StudentProfiles
            .OrderBy(s => s.Name)
            .ToListAsync();
        return View(students);
    }

    // GET: Students/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var student = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.Id == id);
        if (student == null) return NotFound();
        return View(student);
    }

    // GET: Students/Create
    public IActionResult Create() => View();

    // POST: Students/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("StudentNumber,Name,Email,Phone,Address,DOB")] StudentProfile student)
    {
        // Navigation properties are not posted — remove them from ModelState
        ModelState.Remove("IdentityUser");
        ModelState.Remove("IdentityUserId");
        ModelState.Remove("Enrolments");
        ModelState.Remove("AssignmentResults");
        ModelState.Remove("ExamResults");

        if (ModelState.IsValid)
        {
            // IdentityUserId left empty — admin creates profile,
            // can be linked to a login account later via seed or manually
            student.IdentityUserId = "";
            _context.Add(student);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(student);
    }

    // GET: Students/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var student = await _context.StudentProfiles.FindAsync(id);
        if (student == null) return NotFound();
        return View(student);
    }

    // POST: Students/Edit/5
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

    // GET: Students/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var student = await _context.StudentProfiles.FirstOrDefaultAsync(s => s.Id == id);
        if (student == null) return NotFound();
        return View(student);
    }

    // POST: Students/Delete/5
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