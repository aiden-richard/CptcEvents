using System;
using System.Collections.Generic;

namespace CptcEvents.Models;

/// <summary>
/// View model for the group management dashboard.
/// </summary>
public class ManageGroupViewModel
{
    public required GroupSummaryViewModel Group { get; set; }

    public string? Description { get; set; }

    public PrivacyLevel PrivacyLevel { get; set; }

    public bool UserIsOwner { get; set; }

    public bool UserIsModerator { get; set; }

    public bool ModeratorsCanInvite { get; set; }

    public int MemberCount { get; set; }

    public int ModeratorCount { get; set; }

    public int InviteCount { get; set; }

    public int UpcomingEventCount { get; set; }

    public List<GroupEventListItemViewModel> UpcomingEvents { get; set; } = new();
}
