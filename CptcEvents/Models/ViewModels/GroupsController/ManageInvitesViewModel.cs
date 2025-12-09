using System;
using System.Collections.Generic;

namespace CptcEvents.Models;

/// <summary>
/// View model for listing and revoking invites for a group.
/// </summary>
public class ManageInvitesViewModel
{
    public required GroupSummaryViewModel Group { get; set; }

    public bool UserIsOwner { get; set; }

    public bool ModeratorsCanInvite { get; set; }

    public List<InviteListItemViewModel> Invites { get; set; } = new();
}

/// <summary>
/// Lightweight invite record for management grids.
/// </summary>
public class InviteListItemViewModel
{
    public int Id { get; set; }

    public string InviteCode { get; set; } = string.Empty;

    public string CreatedBy { get; set; } = string.Empty;

    public string? InvitedUser { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool OneTimeUse { get; set; }

    public int TimesUsed { get; set; }

    public bool IsExpired { get; set; }
}
