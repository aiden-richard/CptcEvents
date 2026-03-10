using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace CptcEvents.Models;

/// <summary>
/// Defines whether a group can be joined without an invite.
/// </summary>
public enum PrivacyLevel
{
    [Display(Name = "Public - Anyone can join")]
    Public,

    [Display(Name = "Private - Requires invite to join")]
    RequiresInvite
}

/// <summary>
/// Defines who is allowed to create invites for a group.
/// </summary>
public enum GroupInvitePolicy
{
    [Display(Name = "Any member can create invites")]
    AnyMember,

    [Display(Name = "Moderators and above can create invites")]
    ModeratorAndAbove,

    [Display(Name = "Only the owner can create invites")]
    OwnerOnly
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
    [StringLength(100)]
    public required string Name { get; set; }

    /// <summary>
    /// Optional description of the group
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key referencing the <see cref="ApplicationUser"/> that owns the group.
    /// </summary>
    [Required]
    public required string OwnerId { get; set; }

    /// <summary>
    /// Navigation property for the group owner.
    /// </summary>
    [ForeignKey(nameof(OwnerId))]
    public ApplicationUser Owner { get; set; } = null!;

    /// <summary>
    /// UTC timestamp when the group was created. Set at construction time by default.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Tracks the privacy level of the group (join access).
    /// </summary>
    [Required]
    public PrivacyLevel PrivacyLevel { get; set; } = PrivacyLevel.RequiresInvite;

    /// <summary>
    /// Defines who is allowed to create invites for this group.
    /// </summary>
    [Required]
    public GroupInvitePolicy InvitePolicy { get; set; } = GroupInvitePolicy.ModeratorAndAbove;

    /// <summary>
    /// Hex color code for the group used in calendar displays.
    /// Defaults to a generated color based on group ID.
    /// </summary>
    [StringLength(7)]
    public string? Color { get; set; }
    
    /// <summary>
    /// Collection of users who are members of the group.
    /// </summary>
    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();

    public int MemberCount => Members.Count;
}
