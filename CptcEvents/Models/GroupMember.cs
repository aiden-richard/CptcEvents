using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CptcEvents.Models;

public enum RoleType
{
    [Display(Name = "Member")]
    Member,

    [Display(Name = "Moderator")]
    Moderator,

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
    /// Foreign key to the <see cref="Group"/> this membership belongs to.
    /// </summary>
    [Required]
    public int GroupId { get; set; }

    /// <summary>
    /// Navigation property for the server.
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
    /// Role of the member within the server (e.g. "member", "admin"). Defaults to "member".
    /// </summary>
    [Required]
    [MaxLength(50)]
    public RoleType Role { get; set; } = RoleType.Member;

    /// <summary>
    /// UTC timestamp when the user joined the server. Defaults to the time the instance is created.
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}