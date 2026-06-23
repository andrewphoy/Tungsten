using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using Tungsten.Data;
using Tungsten.Helpers;
using Tungsten.Models;

namespace Tungsten.Controllers {
    public class RsvpController : Controller {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;


        public RsvpController(ApplicationDbContext dbContext, ILogger<HomeController> logger) {
            _context = dbContext;
            _logger = logger;
        }

        public IActionResult TextPlain(object content) {
            return new ContentResult() {
                Content = content.ToString(),
                ContentType = "text/plain"
            };
        }

        public override void OnActionExecuting(ActionExecutingContext context) {
            base.OnActionExecuting(context);
            ViewData["Title"] = "RSVP";
            ViewData["UseJs"] = true;
        }

        [HttpGet("rsvp")]
        public IActionResult Index() {
            return View("Landing");
        }

        [HttpPost("rsvp")]
        public async Task<IActionResult> IndexPost([FromForm] string guestName) {
            if (string.IsNullOrWhiteSpace(guestName)) {
                return TextPlain("missing guest name, refresh and try again");
            }

            var guest = await _context.Guests.FirstOrDefaultAsync(g => guestName.Equals(g.FirstName + " " + g.LastName));
            if (guest != null) {
                HttpContext.Session.SetInt32("GuestId", guest.Id);
                return Redirect("/rsvp/wedding");
            }

            return TextPlain("Could not find guest");
        }

        private async Task<Tuple<Guest?, List<Guest>?>> GetFamily(int? id) {
            if (!id.HasValue) {
                throw new ArgumentNullException(nameof(id));
            }

            List<Guest>? family = null;

            var guest = await _context.Guests.FindAsync(id.Value);
            if (guest != null && guest.FamilyId.HasValue) {
                family = await _context.Guests.Where(g => g.FamilyId == guest.FamilyId.Value).ToListAsync();
                family = family.Where(f => f.Id != guest.Id).ToList();
            }

            return Tuple.Create(guest, family);
        }


        [HttpGet("rsvp/wedding")]
        public async Task<IActionResult> Attendance() {
            int? guestId = HttpContext.Session.GetInt32("GuestId");
            if (!guestId.HasValue) {
                return Redirect("/rsvp");
            }

            var tuple = await GetFamily(guestId.Value);
            return View(tuple);
        }

        [HttpPost("rsvp/wedding")]
        public async Task<IActionResult> AttendancePost() {
            int? guestId = HttpContext.Session.GetInt32("GuestId");
            if (!guestId.HasValue) {
                return Redirect("/rsvp");
            }

            var tuple = await GetFamily(guestId.Value);
            Dictionary<int, Guest> guests = new Dictionary<int, Guest>();

            foreach (var kvp in HttpContext.Request.Form) {
                string val = kvp.Value.ToString();
                int ixLastDash = kvp.Key.LastIndexOf('-');
                if (ixLastDash < 0) {
                    return TextPlain("error");
                }
                int num = int.Parse(kvp.Key.Substring(ixLastDash + 1));
                if (!guests.ContainsKey(num)) {
                    guests[num] = new Guest {
                        Num = num
                    };
                }

                Guest guest = guests[num];
                string prop = kvp.Key.Substring(0, ixLastDash);
                switch (prop.ToLowerInvariant()) {
                    case "btn-attend":
                        guest.IsAttending = (val == "yes");
                        break;
                    case "guest-name":
                        int ixSpace = val.LastIndexOf(' ');
                        if (ixSpace > 0) {
                            guest.FirstName = val.Substring(0, ixSpace);
                            guest.LastName = val.Substring(ixSpace + 1);
                        } else {
                            guest.FirstName = val;
                            guest.LastName = tuple.Item1!.LastName;
                        }
                        break;
                }
            }


            foreach (var guest in guests.Values) {
                bool found = false;

                if (guest.Num == 1) {
                    // primary guest
                    if (guest.NameMatch(tuple.Item1)) {
                        var dbGuest = tuple.Item1!;
                        dbGuest.IsAttending = guest.IsAttending;
                        if (!dbGuest.RsvpResponseDate.HasValue) {
                            dbGuest.RsvpResponseDate = DateTime.UtcNow;
                        }
                        dbGuest.LastChangeDate = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        found = true;
                    }
                } else if (tuple.Item2 != null && tuple.Item2.Count > 0) {
                    foreach (var fam in tuple.Item2) {
                        if (guest.NameMatch(fam)) {
                            fam.IsAttending = guest.IsAttending;
                            if (!fam.RsvpResponseDate.HasValue) {
                                fam.RsvpResponseDate = DateTime.UtcNow;
                            }
                            fam.LastChangeDate = DateTime.UtcNow;
                            await _context.SaveChangesAsync();
                            found = true;
                            break;
                        }
                    }
                } 

                if (!found) {
                    // new guest
                    var parent = tuple.Item1!;
                    var newGuest = new Guest {
                        FirstName = guest.FirstName,
                        LastName = guest.LastName,
                        FamilyId = parent.FamilyId,
                        EmailAddress = parent.EmailAddress,
                        IsAttending = guest.IsAttending,
                        RsvpResponseDate = DateTime.UtcNow,
                        CreationDate = DateTime.UtcNow,
                        LastChangeDate = DateTime.UtcNow
                    };

                    await _context.Guests.AddAsync(newGuest);
                    await _context.SaveChangesAsync();
                }
            }


            //todo update database
            if (guests.Values.Any(g => g.IsAttending.HasValue && g.IsAttending.Value)) {
                return Redirect("/rsvp/meals");
            } else {
                return Redirect("/rsvp/comments");
            }
        }

        [HttpGet("rsvp/comments")]
        public async Task<IActionResult> Comments() {
            int? guestId = HttpContext.Session.GetInt32("GuestId");
            if (!guestId.HasValue) {
                return Redirect("/rsvp");
            }

            var tuple = await GetFamily(guestId.Value);
            return View(tuple);
        }

        [HttpPost("rsvp/comments")]
        public async Task<IActionResult> CommentsPost() {
            int? guestId = HttpContext.Session.GetInt32("GuestId");
            if (!guestId.HasValue) {
                return Redirect("/rsvp");
            }

            var tuple = await GetFamily(guestId.Value);
            if (HttpContext.Request.Form.ContainsKey("comments")) {
                string? comments = HttpContext.Request.Form["comments"].ToString();

                if (!string.IsNullOrWhiteSpace(comments)) {
                    tuple.Item1!.Comments = comments;
                    await _context.SaveChangesAsync();
                }
            }

            return View("Success");
        }

        [HttpGet("rsvp/meals")]
        public async Task<IActionResult> Meals() {
            int? guestId = HttpContext.Session.GetInt32("GuestId");
            if (!guestId.HasValue) {
                return Redirect("/rsvp");
            }

            var tuple = await GetFamily(guestId.Value);
            return View(tuple);
        }

        [HttpPost("rsvp/meals")]
        public async Task<IActionResult> MealsPost() {
            int? guestId = HttpContext.Session.GetInt32("GuestId");
            if (!guestId.HasValue) {
                return Redirect("/rsvp");
            }

            string? mealComments = null;
            string? songRequest = null;
            string? comments = null;

            var tuple = await GetFamily(guestId.Value);
            Dictionary<int, Guest> guests = new Dictionary<int, Guest>();

            foreach (var kvp in HttpContext.Request.Form) {
                string val = kvp.Value.ToString();
                int ixLastDash = kvp.Key.LastIndexOf('-');
                if (ixLastDash < 0) {
                    switch (kvp.Key) {
                        case "mealcomments":
                            mealComments = val;
                            break;
                        case "songrequest":
                            songRequest = val;
                            break;
                        case "note":
                            comments = val;
                            break;
                    }
                    continue;
                }
                int num = int.Parse(kvp.Key.Substring(ixLastDash + 1));
                if (!guests.ContainsKey(num)) {
                    guests[num] = new Guest {
                        Num = num
                    };
                }

                Guest guest = guests[num];
                string prop = kvp.Key.Substring(0, ixLastDash);
                switch (prop.ToLowerInvariant()) {
                    case "btn-meal":
                        guest.MealChoice = val;
                        break;
                    case "guest-id":
                        guest.Id = int.Parse(val);
                        break;
                    case "check-vegan":
                        guest.IsVegan = !string.IsNullOrWhiteSpace(val);
                        break;
                    case "check-gluten":
                        guest.IsGlutenFree = !string.IsNullOrWhiteSpace(val);
                        break;
                    case "check-kosher":
                        guest.IsKosher = !string.IsNullOrWhiteSpace(val);
                        break;
                }
            }

            foreach (var g in guests.Values) {
                Guest? dbGuest = null;
                if (g.Num == 1) {
                    dbGuest = tuple.Item1;
                } else {
                    if (tuple.Item2 != null && tuple.Item2.Count > 0) {
                        foreach (var fam in tuple.Item2) {
                            if (g.Id == fam.Id) {
                                dbGuest = fam;
                            }
                        }
                    }
                }

                if (dbGuest != null) {
                    dbGuest.MealChoice = g.MealChoice;
                    dbGuest.IsVegan = g.IsVegan;
                    dbGuest.IsGlutenFree = g.IsGlutenFree;
                    dbGuest.IsKosher = g.IsKosher;

                    dbGuest.MealComments = mealComments;
                    dbGuest.SongRequest = songRequest;
                    dbGuest.Comments = comments;
                    await _context.SaveChangesAsync();
                }
            }

            return View("Success");
        }
    }
}
