using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CptcEvents.Models;

public class GroupViewModel
{
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
    /// Tracks the privacy level of the group.
    /// </summary>
    [Required]
    public PrivacyLevel PrivacyLevel { get; set; } = PrivacyLevel.ModeratorInvitePrivate;
}
