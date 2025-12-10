using System;

namespace CptcEvents.Models;

/// <summary>
/// Read model for displaying events within a group context.
/// </summary>
public class GroupEventListItemViewModel
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? GroupName { get; init; }

    public int GroupId { get; init; }

    public DateOnly DateOfEvent { get; init; }

    public TimeOnly StartTime { get; init; }

    public TimeOnly EndTime { get; init; }

    public bool IsAllDay { get; init; }

    public bool IsPublic { get; init; }
    public bool IsApprovedPublic { get; init; }
    public bool IsDeniedPublic { get; init; }

    public string Description { get; init; } = string.Empty;

    public string? Url { get; init; }

    public string CreatedByUserId { get; init; } = string.Empty;
}
