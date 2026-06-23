using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tungsten.Data;
using Tungsten.Helpers;
using Tungsten.Models;

namespace Tungsten.Controllers {

    //[BasicAuthorize(Username = "admin", Password = "AndrewAndHaley'sWedding")]
    public class AdminController : Controller {

        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context) {
            _context = context;
        }

        [HttpGet("admin/responses")]
        public async Task<IActionResult> Responses() {
            var responses = await _context.Guests.Where(g => g.IsAttending.HasValue).OrderByDescending(g => g.RsvpResponseDate).ToListAsync();

            var summary = new RsvpSummary();

            foreach (var rsvp in responses) {
                if (rsvp.IsAttending.HasValue) {
                    if (rsvp.IsAttending.Value) {
                        summary.Attending++;

                        string key = rsvp.MealChoice ?? "";
                        if (!summary.Meals.ContainsKey(key)) {
                            summary.Meals.Add(key, 0);
                        }

                        summary.Meals[key]++;

                    } else {
                        summary.Declined++;
                    }
                } 
            }

            ViewData["Summary"] = summary;

            return View(responses);
        }
    }
}
