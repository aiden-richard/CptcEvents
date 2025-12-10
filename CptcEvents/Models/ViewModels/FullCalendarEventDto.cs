namespace CptcEvents.Models;

/// <summary>
/// DTO returned to FullCalendar consumers instead of loose dictionaries.
/// </summary>
public class FullCalendarEventDto
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Start { get; init; } = string.Empty;

    public string End { get; init; } = string.Empty;

    public bool AllDay { get; init; }

    public int GroupId { get; init; }

    public string? BackgroundColor { get; init; }
}
