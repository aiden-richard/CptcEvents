using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace CptcEvents.Models;

/// <summary>
/// Defines the privacy modes available for a <see cref="Group"/>.
/// </summary>
public enum GroupPrivacy
{
    /// <summary>
    /// The group is visible to everyone and may be joined without approval.
    /// </summary>
    [Display(Name = "Public")]
    Public = 0,

    /// <summary>
    /// The group is visible and users may request to join; membership requires approval.
    /// </summary>
    [Display(Name = "Request to Join")]
    RequestToJoin = 1,

    /// <summary>
    /// The group is invite-only; users can only join by invitation.
    /// </summary>
    [Display(Name = "Invite Only")]
    InviteOnly = 2
}

/// <summary>
/// Represents a community group with members, description and privacy settings.
/// </summary>
public class Group
{
    /// <summary>
    /// Primary key for the group.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// The display name of the group.
    /// </summary>
    [Required]
    public required string Name { get; set; }

    /// <summary>
    /// A longer description of the group's purpose.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Tracks the privacy level of the group.
    /// </summary>
    [Required]
    public GroupPrivacy Privacy { get; set; } = GroupPrivacy.Public;
    
    /// <summary>
    /// Collection of users who are members of the group.
    /// </summary>
    public List<ApplicationUser> Members { get; set; } = new();

    // Convenience helpers (not mapped to the DB)

    /// <summary>
    /// Indicates if the group is public (no approval required to join).
    /// This property is not persisted to the database.
    /// </summary>
    [NotMapped]
    public bool IsPublic => Privacy == GroupPrivacy.Public;

    /// <summary>
    /// Indicates if joining the group requires approval.
    /// This property is not persisted to the database.
    /// </summary>
    [NotMapped]
    public bool RequiresApproval => Privacy == GroupPrivacy.RequestToJoin;

    /// <summary>
    /// Indicates if the group is invite-only.
    /// This property is not persisted to the database.
    /// </summary>
    [NotMapped]
    public bool IsInviteOnly => Privacy == GroupPrivacy.InviteOnly;
}
