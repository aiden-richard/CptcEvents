using CptcEventHub.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using CptcEvents.Services;

namespace CptcEventHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEventService _eventsService;

        public HomeController(IEventService eventsService)
        {
            _eventsService = eventsService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
