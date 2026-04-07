using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VgcCollege.Web.Models;

namespace VgcCollege.Web.Controllers;

public class HomeController : Controller
{
    // GET: /
    // Redirects logged-in users to their role dashboard immediately.
    // Unauthenticated users see the welcome page.
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction("Index", "Admin");

            if (User.IsInRole("Faculty"))
                return RedirectToAction("MyStudents", "Faculty");

            if (User.IsInRole("Student"))
                return RedirectToAction("MyProfile", "Student");
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}