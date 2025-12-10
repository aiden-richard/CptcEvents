using System.ComponentModel.DataAnnotations;

namespace CptcEvents.Models;

/// <summary>
/// Represents an instructor code used for registration.
/// </summary>
public class InstructorCode
{
    /// <summary>
    /// Primary key for the instructor code record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The unique code string used for registration.
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// The email address associated with this instructor code.
    /// </summary>
    [EmailAddress]
    [Required]
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this code is currently active and can be used.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// UTC timestamp when the code expires and can no longer be used. Null for codes that never expire.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// The user ID of who created this instructor code.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// UTC timestamp when the code was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The user ID of who used this instructor code for registration. Null if the code hasn't been used yet.
    /// </summary>
    public string? UsedByUserId { get; set; }

    /// <summary>
    /// UTC timestamp when the code was used. Null if the code hasn't been used yet.
    /// </summary>
    public DateTime? UsedAt { get; set; }
}