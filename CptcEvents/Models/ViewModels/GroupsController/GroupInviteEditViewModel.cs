using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CptcEvents.Models;

/// <summary>
/// View model for editing an existing group invite.
/// </summary>
public class GroupInviteEditViewModel : IValidatableObject
{
    [Required]
    public int Id { get; set; }

    [Required]
    public int GroupId { get; set; }

    [Display(Name = "Expires")]
    public bool Expires { get; set; }

    [Display(Name = "Expires At")]
    public DateTime? ExpiresAt { get; set; }

    [Display(Name = "One Time Use")]
    public bool OneTimeUse { get; set; }

    public string? InviteCode { get; set; }

    public string? InvitedUserDisplay { get; set; }

    /// <summary>
    /// Converts the local expiration value to UTC for persistence.
    /// </summary>
    public DateTime? ExpiresAtUtc => Expires && ExpiresAt.HasValue
        ? DateTime.SpecifyKind(ExpiresAt.Value, DateTimeKind.Local).ToUniversalTime()
        : null;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Expires)
        {
            if (!ExpiresAt.HasValue)
            {
                yield return new ValidationResult("Expiration date/time must be provided when expiry is enabled.", new[] { nameof(ExpiresAt) });
            }
            else if (ExpiresAtUtc <= DateTime.UtcNow)
            {
                yield return new ValidationResult("Expiration must be a future date/time.", new[] { nameof(ExpiresAt) });
            }
        }
    }
}
