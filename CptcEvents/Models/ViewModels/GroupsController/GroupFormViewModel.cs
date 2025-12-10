using System.ComponentModel.DataAnnotations;

namespace CptcEvents.Models;

/// <summary>
/// Form model for creating or editing a group.
/// </summary>
public class GroupFormViewModel
{
    /// <summary>
    /// Group ID when editing; null when creating.
    /// </summary>
    public int? Id { get; set; }

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
    public string? Description { get; set; } = string.Empty;

    /// <summary>
    /// Privacy level for the group.
    /// </summary>
    [Required]
    public PrivacyLevel PrivacyLevel { get; set; } = PrivacyLevel.ModeratorInvitePrivate;

    /// <summary>
    /// Hex color code for the group used in calendar displays.
    /// </summary>
    [StringLength(7)]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex code (e.g., #0d6efd)")]
    public string? Color { get; set; }

    /// <summary>
    /// Indicates whether the current user is the owner (UI gating only).
    /// </summary>
    public bool IsOwner { get; set; }
}
