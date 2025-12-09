using System;
using System.Collections.Generic;

namespace CptcEvents.Models;

/// <summary>
/// View model for managing group membership and moderator roles.
/// </summary>
public class ManageMembersViewModel
{
    public required GroupSummaryViewModel Group { get; set; }

    public bool UserIsOwner { get; set; }

    public List<ManageMemberListItemViewModel> Members { get; set; } = new();
}

/// <summary>
/// Row-level data for member management.
/// </summary>
public class ManageMemberListItemViewModel
{
    public required string UserId { get; set; }

    public required string DisplayName { get; set; }

    public required string UserName { get; set; }

    public RoleType Role { get; set; }

    public DateTime JoinedAt { get; set; }

    public bool IsCurrentUser { get; set; }

    public bool CanPromote { get; set; }

    public bool CanDemote { get; set; }

    public bool CanRemove { get; set; }
}
