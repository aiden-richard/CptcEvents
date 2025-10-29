using System.ComponentModel.DataAnnotations;

namespace CptcEventHub.Models;

/// <summary>
/// This class represents an event in the website.
/// It is used to store information about events such as title, description, date, and location.
/// Some events are public and some are private.
/// </summary>
public class Event
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    public bool IsPublic { get; set; } = false;

    public bool IsAllDay { get; set; } = true;

    /// <summary>
    /// Day the event is taking place
    /// </summary>
    [Required]
    public DateOnly DateOfEvent { get; set; }

    /// <summary>
    /// Start time of the event
    /// </summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>
    /// End time of the event
    /// </summary>
    public TimeOnly EndTime { get; set; }

    [StringLength(2083)]
    [DataType(DataType.Url)]
    [Url]
    public string? Url { get; set; }

    /// <summary>
    /// Gets the starting date and time of the event by combining DateOfEvent and StartTime.
    /// If IsAllDay is true, returns the start of the day (00:00).
    /// </summary>
    public DateTime StartingDateTime
    {
        get
        {
            return IsAllDay ? DateOfEvent.ToDateTime(new TimeOnly(0, 0)) : DateOfEvent.ToDateTime(StartTime);
        }
    }

    /// <summary>
    /// Gets the ending date and time of the event by ombining DateOfEvent and EndTime.
    /// If IsAllDay is true, returns the end of the day (23:59).
    /// </summary>
    public DateTime EndingDateTime
    {
        get
        {
            return IsAllDay ? DateOfEvent.ToDateTime(new TimeOnly(23, 59)) : DateOfEvent.ToDateTime(EndTime);
        }
    }

    /// <summary>
    /// Builds a FullCalendar-compatible object representing this event.
    /// Returns a dictionary that will be serialized by MVC into a JSON object
    /// instead of returning a pre-serialized JSON string (which becomes a JSON string value).
    /// </summary>
    public object ToFullCalendarEvent()
    {
        var obj = new Dictionary<string, object?>
        {
            ["id"] = Id,
            ["title"] = Title,
        };

        if (IsAllDay)
        {
            // FullCalendar supports all-day events with a date string
            obj["start"] = DateOfEvent;
        }
        else
        {
            // Provide ISO-8601 datetimes for start/end when not all-day
            obj["start"] = StartingDateTime.ToString("s");
            obj["end"] = EndingDateTime.ToString("s");
        }

        // Include URL if provided
        // not included for now until we need it
        //if (!string.IsNullOrWhiteSpace(Url))
        //{
        //    obj["url"] = Url;
        //}

        return obj;
    }
}
