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
public enum PrivacyLevel
{
    [Display(Name = "Public - Anyone can join")]
    Public,

    [Display(Name = "Moderators and above can create invites")]
    ModeratorInvitePrivate,

    [Display(Name = "Only owner can create invites")]
    OwnerInvitePrivate
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
    /// Tracks the privacy level of the group.
    /// </summary>
    [Required]
    public PrivacyLevel PrivacyLevel { get; set; } = PrivacyLevel.ModeratorInvitePrivate;
    
    /// <summary>
    /// Collection of users who are members of the group.
    /// </summary>
    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();

    public int MemberCount => Members.Count;
}
