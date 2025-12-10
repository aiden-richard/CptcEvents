using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CptcEvents.Models;

/// <summary>
/// Defines the role a user can have within a group.
/// </summary>
public enum RoleType
{
    /// <summary>
    /// Standard member with limited permissions.
    /// </summary>
    [Display(Name = "Member")]
    Member,

    /// <summary>
    /// Moderator with elevated permissions to manage group content and members.
    /// </summary>
    [Display(Name = "Moderator")]
    Moderator,

    /// <summary>
    /// Owner of the group with full administrative control.
    /// </summary>
    [Display(Name = "Owner")]
    Owner
}

/// <summary>
/// Represents a user's membership in a group, including their role and join time.
/// </summary>
public class GroupMember
{
    /// <summary>
    /// Primary key for the membership record.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the <see cref="GroupInvite"/> that was used to join, if applicable.
    /// </summary>
    public int? InviteId { get; set; }

    /// <summary>
    /// Navigation property for the invite that created this membership, if any.
    /// </summary>
    [ForeignKey(nameof(InviteId))]
    public GroupInvite? Invite { get; set; }

    /// <summary>
    /// Foreign key to the <see cref="Group"/> this membership belongs to.
    /// </summary>
    [Required]
    public int GroupId { get; set; }

    /// <summary>
    /// Navigation property for the group.
    /// </summary>
    [ForeignKey(nameof(GroupId))]
    public Group Group { get; set; } = null!;

    /// <summary>
    /// Foreign key to the <see cref="ApplicationUser"/> who is the member.
    /// </summary>
    [Required]
    public required string UserId { get; set; }

    /// <summary>
    /// Navigation property for the user who is the member.
    /// </summary>
    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Role of the member within the group from <see cref="RoleType"/>.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public RoleType Role { get; set; } = RoleType.Member;

    /// <summary>
    /// UTC timestamp when the user joined the group. Defaults to the time the instance is created.
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}