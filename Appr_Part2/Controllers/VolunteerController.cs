using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Appr_Part2.Models;

namespace Appr_Part2.Controllers
{
    [Authorize]
    public class VolunteerController : Controller
    {
        private readonly Appr_Part2_DbContext _context;

        public VolunteerController(Appr_Part2_DbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var volunteers = await _context.Volunteers
                .Include(v => v.User)
                .Where(v => v.Status == "Active")
                .ToListAsync();

            return View(volunteers);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(VolunteerViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                    // Check if user is already registered as volunteer
                    var existingVolunteer = await _context.Volunteers
                        .FirstOrDefaultAsync(v => v.UserId == userId);

                    if (existingVolunteer != null)
                    {
                        TempData["ErrorMessage"] = "You are already registered as a volunteer.";
                        return RedirectToAction("MyRegistration");
                    }

                    // Create new Volunteer from ViewModel
                    var volunteer = new Volunteer
                    {
                        Skills = model.Skills,
                        Availability = model.Availability,
                        HasTransportation = model.HasTransportation,
                        PreferredLocation = model.PreferredLocation,
                        EmergencyContact = model.EmergencyContact,
                        UserId = userId,
                        RegisteredAt = DateTime.UtcNow,
                        Status = "Active"
                    };

                    _context.Volunteers.Add(volunteer);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Volunteer registration successful! Thank you for your commitment.";
                    return RedirectToAction("MyRegistration");
                }
                catch (Exception ex)
                {
                    // Log the actual error for debugging
                    Console.WriteLine($"Error saving volunteer: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }

                    ModelState.AddModelError("", "An error occurred while saving your volunteer registration. Please try again.");
                }
            }

            // If we got this far, something failed; redisplay form
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> MyRegistration()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var volunteer = await _context.Volunteers
                .FirstOrDefaultAsync(v => v.UserId == userId);

            return View(volunteer);
        }
    }
}