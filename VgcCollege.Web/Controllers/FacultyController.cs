using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VgcCollege.Web.Controllers;

[Authorize(Roles = "Faculty")]
public class FacultyController : Controller
{
    public IActionResult MyStudents() => View();
    public IActionResult Gradebook() => View();
}