using System.ComponentModel.DataAnnotations;
using System.Text.Json;

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

    // Convert DateOnly and TimeOnly to DateTime
    public DateTime StartingDateTime
    {
        get
        {
            return IsAllDay ? DateOfEvent.ToDateTime(new TimeOnly(0, 0)) : DateOfEvent.ToDateTime(StartTime);
        }
    }

    public DateTime EndingDateTime
    {
        get
        {
            return IsAllDay ? DateOfEvent.ToDateTime(new TimeOnly(23, 59)) : DateOfEvent.ToDateTime(EndTime);
        }
    }

    /// <summary>
    /// Builds a FullCalendar-compatible JSON object string representing this event.
    /// Example output: {"title":"The Title","start":"2018-09-01T09:00:00","end":"2018-09-01T11:00:00","url":"https://..."}
    /// Uses ISO-8601 datetime format which FullCalendar can parse. If `Url` is not set it will be omitted.
    /// </summary>
    public string ToFullCalendarEventJson()
    {
        var obj = new Dictionary<string, object?>
        {
            ["title"] = Title,
            ["description"] = Description,
            // Use sortable ISO format without timezone: yyyy-MM-ddTHH:mm:ss
            ["start"] = StartingDateTime.ToString("s"),
            ["end"] = EndingDateTime.ToString("s")
        };

        if (!string.IsNullOrWhiteSpace(Url))
        {
            obj["url"] = Url;
        }

        var options = new JsonSerializerOptions
        {
            // Keep property names as provided (already camel-case keys)
            WriteIndented = false
        };

        return JsonSerializer.Serialize(obj, options);
    }
}
