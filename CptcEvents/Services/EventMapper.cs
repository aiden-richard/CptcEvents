using CptcEvents.Models;

namespace CptcEvents.Services
{
    public static class EventMapper
    {
        public static object ToFullCalendarEvent(Event e)
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

            return obj;
        }
    }
}
