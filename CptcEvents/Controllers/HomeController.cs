using CptcEvents.Models;
using CptcEvents.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CptcEvents.Controllers
{
    /// <summary>
    /// Controller for displaying public-facing home and informational pages.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IEventService _eventService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(IEventService eventService, UserManager<ApplicationUser> userManager)
        {
            _eventService = eventService;
            _userManager = userManager;
        }

        /// <summary>
        /// Displays the home page with approved public events.
        /// GET /
        /// </summary>
        /// <returns>Home page view.</returns>
        public async Task<IActionResult> Index()
        {
            return View();
        }

        /// <summary>
        /// Displays the privacy policy page.
        /// GET /Home/Privacy
        /// </summary>
        /// <returns>Privacy policy view.</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Displays the error page when an exception occurs.
        /// GET /Home/Error
        /// </summary>
        /// <returns>Error view with request ID.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
