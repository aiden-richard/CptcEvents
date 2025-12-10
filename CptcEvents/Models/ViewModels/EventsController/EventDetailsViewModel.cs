using System;

namespace CptcEvents.Models;

/// <summary>
/// Read model for event detail and delete views.
/// </summary>
public class EventDetailsViewModel
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string? GroupName { get; init; }

    public int GroupId { get; init; }

    public DateOnly DateOfEvent { get; init; }

    public TimeOnly StartTime { get; init; }

    public TimeOnly EndTime { get; init; }

    public bool IsAllDay { get; init; }

    public bool IsPublic { get; init; }

    public string? Url { get; init; }
}
