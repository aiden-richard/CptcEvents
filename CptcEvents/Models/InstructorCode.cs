using System.ComponentModel.DataAnnotations;

namespace CptcEvents.Models;

/// <summary>
/// Represents an instructor code used for registration.
/// </summary>
public class InstructorCode
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [EmailAddress]
    [Required]
    public required string Email { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? ExpiresAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}