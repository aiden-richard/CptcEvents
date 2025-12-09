using System.Collections.Generic;

namespace CptcEvents.Models;

/// <summary>
/// View model for displaying or managing a group's events with moderator-aware UI elements.
/// </summary>
public class GroupEventsViewModel
{
    /// <summary>
    /// The group whose events are being displayed.
    /// </summary>
    public required GroupSummaryViewModel Group { get; set; }

    /// <summary>
    /// Indicates whether the current user can edit events in the group.
    /// </summary>
    public bool UserCanEditEvents { get; set; }

    /// <summary>
    /// Next upcoming events for the group (future only, limited) when showing the overview.
    /// </summary>
    public List<GroupEventListItemViewModel> UpcomingEvents { get; set; } = new();

    /// <summary>
    /// Full event list used in manage mode.
    /// </summary>
    public List<GroupEventListItemViewModel> Events { get; set; } = new();

    /// <summary>
    /// Indicates whether the page is in manage mode (EditEvents) or overview mode (Events).
    /// </summary>
    public bool IsManageMode { get; set; }
}
