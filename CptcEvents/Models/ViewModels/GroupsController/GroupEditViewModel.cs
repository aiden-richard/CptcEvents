using System.ComponentModel.DataAnnotations;

namespace CptcEvents.Models;

/// <summary>
/// View model for editing an existing group.
/// </summary>
public class GroupEditViewModel
{
    /// <summary>
    /// The ID of the group being edited.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The display name of the group.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Name { get; set; }

    /// <summary>
    /// Optional description of the group.
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Tracks the privacy level of the group.
    /// Only owners can change this.
    /// </summary>
    [Required]
    public PrivacyLevel PrivacyLevel { get; set; } = PrivacyLevel.ModeratorInvitePrivate;

    /// <summary>
    /// Indicates whether the current user is the owner (used for UI display).
    /// </summary>
    public bool IsOwner { get; set; }
}
