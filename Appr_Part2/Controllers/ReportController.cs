using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Appr_Part2.Models;

namespace Appr_Part2.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly Appr_Part2_DbContext _context;

        public ReportController(Appr_Part2_DbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var incidents = await _context.IncidentReports
                .Include(i => i.User)
                .OrderByDescending(i => i.ReportedAt)
                .ToListAsync();

            return View(incidents);
        }

        [HttpGet]
        public IActionResult Report()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Report(IncidentReportViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                    // Create new IncidentReport from ViewModel
                    var incident = new IncidentReport
                    {
                        Title = model.Title,
                        Description = model.Description,
                        Location = model.Location,
                        IncidentDate = model.IncidentDate,
                        DisasterType = model.DisasterType,
                        AffectedAreas = model.AffectedAreas,
                        UrgencyLevel = model.UrgencyLevel,
                        UserId = userId,
                        ReportedAt = DateTime.UtcNow,
                        Status = "Pending"
                    };

                    _context.IncidentReports.Add(incident);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Incident report submitted successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Log the actual error for debugging
                    Console.WriteLine($"Error saving incident: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }

                    ModelState.AddModelError("", "An error occurred while saving the incident report. Please try again.");
                }
            }

            // If we got this far, something failed; redisplay form
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var incident = await _context.IncidentReports
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.IncidentId == id);

            if (incident == null)
            {
                return NotFound();
            }

            return View(incident);
        }
    }
}