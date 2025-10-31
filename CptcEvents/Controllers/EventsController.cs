using CptcEvents.Models;
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
        /// Retrieves all public calendar events and returns their data formatted for FullCalendar.
        /// </summary>
        /// <remarks>This method is intended for use by pages that require event data
        /// formatted for the FullCalendar JavaScript library. The returned list will be empty if no events are
        /// found.</remarks>
        /// <returns>A JSON result containing a list of event objects formatted for FullCalendar.</returns>
        public async Task<IActionResult> GetEvents()
        {
            // Get events from the database
            IEnumerable<Event> events = await _eventsService.GetPublicEventsAsync();

            // Create a list to hold FullCalendar-compatible events
            List<object> fullCalendarEvents = new List<object>();

            // Loop through the events and add them to the list
            foreach (Event e in events)
            {
                fullCalendarEvents.Add(ToFullCalendarEvent(e));
            }

            return Json(fullCalendarEvents);
        }

        /// <summary>
        /// Builds a FullCalendar-compatible object representing this event.
        /// Returns a dictionary that will be serialized by MVC into a JSON object
        /// instead of returning a pre-serialized JSON string (which becomes a JSON string value).
        /// </summary>
        public object ToFullCalendarEvent(Event e)
        {
            var obj = new Dictionary<string, object?>
            {
                ["id"] = e.Id,
                ["title"] = e.Title,
            };

            if (e.IsAllDay)
            {
                // FullCalendar supports all-day events with a date string
                obj["start"] = e.DateOfEvent;
                obj["end"] = e.DateOfEvent.AddDays(1);
            }
            else
            {
                // Provide ISO-8601 datetimes for start/end when not all-day
                obj["start"] = e.DateOfEvent.ToDateTime(e.StartTime).ToString("s");
                obj["end"] = e.DateOfEvent.ToDateTime(e.EndTime).ToString("s");
            }

            // Include URL if provided
            // *not included for now until we need it
            // fullcalendar will make the title a link if url is provided*
            //if (!string.IsNullOrWhiteSpace(Url))
            //{
            //    obj["url"] = Url;
            //}

            return obj;
        }
    }
}
