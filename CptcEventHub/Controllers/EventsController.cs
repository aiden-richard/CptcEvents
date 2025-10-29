using CptcEventHub.Models;
using CptcEvents.Services;
using Microsoft.AspNetCore.Mvc;

namespace CptcEvents.Controllers
{
    public class EventsController : Controller
    {
        private readonly IEventService _eventsService;

        public EventsController(IEventService eventsService)
        {
            _eventsService = eventsService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Event newEvent)
        {
            if (ModelState.IsValid)
            {
                await _eventsService.AddEventAsync(newEvent);
                return RedirectToAction("Index");
            }
            return View(newEvent);
        }

        /// <summary>
        /// Retrieves all calendar events and returns their data formatted for FullCalendar.
        /// </summary>
        /// <remarks>This method is intended for use by pages that require event data
        /// formatted for the FullCalendar JavaScript library. The returned list will be empty if no events are
        /// found.</remarks>
        /// <returns>A JSON result containing a list of event objects formatted for FullCalendar.</returns>
        public async Task<IActionResult> GetEvents()
        {
            // Get events from the database
            IEnumerable<Event> events = await _eventsService.GetAllEventsAsync();

            // Create a list to hold FullCalendar-compatible events
            List<object> fullCalendarEvents = new List<object>();

            // Loop through the events and add them to the list
            foreach (Event e in events)
            {
                fullCalendarEvents.Add(e.ToFullCalendarEvent());
            }

            return Json(fullCalendarEvents);
        }
    }
}
