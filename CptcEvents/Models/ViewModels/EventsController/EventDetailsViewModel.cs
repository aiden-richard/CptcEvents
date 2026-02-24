using System;

namespace CptcEvents.Models;

/// <summary>
/// Read model for event detail and delete views.
/// </summary>
public record EventDetailsViewModel
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; } = string.Empty;

    public string? GroupName { get; init; }

    public int GroupId { get; init; }

    public DateOnly DateOfEvent { get; init; }

    public TimeOnly StartTime { get; init; }

    public TimeOnly EndTime { get; init; }

    public bool IsAllDay { get; init; }

    public bool IsPublic { get; init; }

    public bool IsApprovedPublic { get; init; }

    public bool IsDeniedPublic { get; init; }

    public string? Url { get; init; }

    public string? BannerImageUrl { get; init; }

    public bool IsCurrentUserMember { get; init; }

    public bool CanEdit { get; init; }

    /// <summary>
    /// Whether RSVP is enabled for this event.
    /// </summary>
    public bool IsRsvpEnabled { get; init; }

    // ── RSVP ──────────────────────────────────────────────

    /// <summary>
    /// The current authenticated user's RSVP status for this event, or null if they haven't RSVP'd.
    /// </summary>
    public RsvpStatus? CurrentUserRsvpStatus { get; init; }

    /// <summary>
    /// The ID of the current user's RSVP record, or null if they haven't RSVP'd.
    /// </summary>
    public int? CurrentUserRsvpId { get; init; }

    /// <summary>
    /// Number of users who RSVP'd "Going" for this event.
    /// </summary>
    public int GoingCount { get; init; }

    /// <summary>
    /// Number of users who RSVP'd "Maybe" for this event.
    /// </summary>
    public int MaybeCount { get; init; }

    /// <summary>
    /// Number of users who RSVP'd "Not Going" for this event.
    /// </summary>
    public int NotGoingCount { get; init; }
}
