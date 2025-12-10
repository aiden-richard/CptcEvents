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
    public bool UserIsModerator { get; set; }

    /// <summary>
    /// Indicates whether the current user is the owner of the group.
    /// </summary>
    public bool UserIsOwner { get; set; }

    /// <summary>
    /// Indicates whether moderators are allowed to create invites for the group.
    /// </summary>
    public bool ModeratorsCanInvite { get; set; }

    /// <summary>
    /// Next upcoming events for the group (future only, limited) when showing the overview.
    /// </summary>
    public List<GroupEventListItemViewModel> UpcomingEvents { get; set; } = new();

    /// <summary>
    /// Full event list used in manage mode.
    /// </summary>
    public List<GroupEventListItemViewModel> Events { get; set; } = new();
}
