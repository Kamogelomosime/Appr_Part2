using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Appr_Part2;
using Appr_Part2.Models;

namespace Appr_Part2.Controllers
{
    [Authorize]
    public class DonationController : Controller
    {
        private readonly Appr_Part2_DbContext _context;

        public DonationController(Appr_Part2_DbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index2()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var donations = await _context.Donations
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();

            return View(donations);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DonationViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                    // Create new Donation from ViewModel
                    var donation = new Donation
                    {
                        DonationType = model.DonationType,
                        ItemDescription = model.ItemDescription,
                        Quantity = model.Quantity,
                        Unit = model.Unit,
                        TargetArea = model.TargetArea,
                        SpecialInstructions = model.SpecialInstructions,
                        UserId = userId,
                        DonationDate = DateTime.UtcNow,
                        Status = "Pending"
                    };

                    _context.Donations.Add(donation);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Donation submitted successfully! Thank you for your generosity.";
                    return RedirectToAction("Index2");
                }
                catch (Exception ex)
                {
                    // Log the actual error for debugging
                    Console.WriteLine($"Error saving donation: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }

                    ModelState.AddModelError("", "An error occurred while saving your donation. Please try again.");
                }
            }

            // If we got this far, something failed; redisplay form
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AllDonations()
        {
            var donations = await _context.Donations
                .Include(d => d.User)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();

            return View(donations);
        }
    }
}