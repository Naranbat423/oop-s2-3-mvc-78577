using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VgcCollege.Domain.Models;
using VgcCollege.Web.Data;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Admin")]
public class BranchesController : Controller
{
    private readonly ApplicationDbContext _context;

    public BranchesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Branches
    public async Task<IActionResult> Index()
    {
        var branches = await _context.Branches.ToListAsync();
        return View(branches);
    }

    // GET: Branches/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == id);
        if (branch == null) return NotFound();

        return View(branch);
    }

    // GET: Branches/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Branches/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Address")] Branch branch)
    {
        if (ModelState.IsValid)
        {
            _context.Add(branch);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(branch);
    }

    // GET: Branches/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var branch = await _context.Branches.FindAsync(id);
        if (branch == null) return NotFound();
        return View(branch);
    }

    // POST: Branches/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address")] Branch branch)
    {
        if (id != branch.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(branch);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BranchExists(branch.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(branch);
    }

    // GET: Branches/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == id);
        if (branch == null) return NotFound();

        return View(branch);
    }

    // POST: Branches/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch != null)
        {
            _context.Branches.Remove(branch);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool BranchExists(int id)
    {
        return _context.Branches.Any(e => e.Id == id);
    }
}