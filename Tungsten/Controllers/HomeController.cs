using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Tungsten.Models;

namespace Tungsten.Controllers {
    public class HomeController : Controller {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger) {
            _logger = logger;
        }

        [Route("", Order = 99)]
        public IActionResult Index() {
            return View();
        }

        [Route("our-story")]
        public IActionResult OurStory() {
            return View();
        }

        [Route("wedding-party")]
        public IActionResult WeddingParty() {
            return View();
        }

        [Route("travel")]
        public IActionResult Travel() {
            return View();
        }

        [Route("things-to-do")]
        public IActionResult ThingsToDo() {
            return View();
        }

        [Route("registry")]
        public IActionResult Registry() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
