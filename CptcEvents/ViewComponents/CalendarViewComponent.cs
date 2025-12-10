using CptcEvents.Models;
using Microsoft.AspNetCore.Mvc;

namespace CptcEvents.ViewComponents
{
    /// <summary>
    /// Renders a reusable FullCalendar instance.
    /// </summary>
    public class CalendarViewComponent : ViewComponent
    {
        public Task<IViewComponentResult> InvokeAsync(string? eventsUrl = null, string? elementId = null, string? initialView = null)
        {
            var model = new CalendarViewModel
            {
                EventsUrl = !string.IsNullOrWhiteSpace(eventsUrl)
                    ? eventsUrl
                    : Url.Action("GetEvents", "Events") ?? "/Events/GetEvents",
                ElementId = string.IsNullOrWhiteSpace(elementId) ? "calendar" : elementId,
                InitialView = string.IsNullOrWhiteSpace(initialView) ? "dayGridMonth" : initialView
            };

            return Task.FromResult<IViewComponentResult>(View(model));
        }
    }
}
