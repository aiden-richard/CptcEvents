using System.ComponentModel.DataAnnotations;

namespace CptcEvents.Models;

/// <summary>
/// Configuration passed to the shared calendar view component.
/// </summary>
public class CalendarViewModel
{
    /// <summary>
    /// Endpoint that returns events in FullCalendar format.
    /// </summary>
    [Required]
    public string EventsUrl { get; set; } = string.Empty;

    /// <summary>
    /// The element id used for the calendar container.
    /// </summary>
    public string ElementId { get; set; } = "calendar";

    /// <summary>
    /// The initial FullCalendar view (e.g., "dayGridMonth", "timeGridWeek").
    /// </summary>
    public string InitialView { get; set; } = "dayGridMonth";
}
