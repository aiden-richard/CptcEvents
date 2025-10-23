using CptcEventHub.Models;
using CptcEvents.Services;
using Microsoft.AspNetCore.Mvc;

namespace CptcEvents.Controllers
{
    public class Events : Controller
    {
        private readonly IEventService _eventsService;

        public Events(IEventService eventsService)
        {
            _eventsService = eventsService;
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Retrieves all calendar events and returns them in a format compatible with FullCalendar.
        /// Links, emails, and phone numbers in event descriptions are converted to clickable HTML links.
        /// </summary>
        /// <remarks>This method is intended for use by pages that require event data
        /// formatted for the FullCalendar JavaScript library. The returned list will be empty if no events are
        /// found.</remarks>
        /// <returns>A JSON result containing a list of event objects, where each object includes the event title, start and end
        /// date-times in ISO 8601 format, and a flag indicating whether the event is a PC2 event.</returns>
        public async Task<IActionResult> GetEvents()
        {
            // Get events from the database
            IEnumerable<Event> events = await _eventsService.GetAllEventsAsync();

            // Create a list to hold FullCalendar-compatible events
            List<object> fullCalendarEvents = new List<object>();

            // Loop through the events and add them to the list
            foreach (Event e in events)
            {
                fullCalendarEvents.Add(e.ToFullCalendarEventJson());
            }

            return Json(fullCalendarEvents);
        }
    }
}
