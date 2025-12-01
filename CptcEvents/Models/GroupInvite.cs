using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CptcEvents.Models;

/// <summary>
/// Represents an invitation to join a <see cref="Group"/>.
/// </summary>
[Index(nameof(InviteCode), IsUnique = true)]
public class GroupInvite : IValidatableObject
{
    /// <summary>
	/// Primary key for the group invite record.
	/// </summary>
	[Key]
	public int Id { get; set; }

	/// <summary>
	/// Foreign key to the <see cref="Group"/> this invite is for.
	/// </summary>
	[Required]
	public int GroupId { get; set; }

	/// <summary>
	/// Navigation property for the group.
	/// </summary>
	[ForeignKey(nameof(GroupId))]
	public Group Group { get; set; } = null!;

	/// <summary>
	/// Foreign key to the <see cref="ApplicationUser"/> who created this invite.
	/// </summary>
	[Required]
	public string CreatedById { get; set; } = null!;

	/// <summary>
	/// Navigation property for the user who created the invite.
	/// </summary>
	[ForeignKey(nameof(CreatedById))]
	public ApplicationUser CreatedBy { get; set; } = null!;

	/// <summary>
	/// Foreign key to the <see cref="ApplicationUser"/> who is invited to the group (optional).
	/// When <c>null</c>, the invite is a general invite (not targeted to a specific user).
	/// </summary>
	public string? InvitedUserId { get; set; }

	/// <summary>
	/// Navigation property for the invited user (optional).
	/// </summary>
	[ForeignKey(nameof(InvitedUserId))]
	public ApplicationUser? InvitedUser { get; set; }

	/// <summary>
	/// A randomly generated code used to join the group via this invite.
	/// The code is a string of capital letters and digits.
	/// </summary>
	[Required]
	public string InviteCode { get; set; } = null!;

	/// <summary>
	/// UTC timestamp when the invite was created.
	/// </summary>
    [Required]
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	/// <summary>
	/// UTC timestamp when the invite expires and can no longer be used.
    /// This is an optional property and can be nulled for non-expiring invites
	/// </summary>
	public DateTime? ExpiresAt { get; set; }

	/// <summary>
	/// If true, the invite can only be used once and becomes invalid after a successful use.
	/// </summary>
	public bool OneTimeUse { get; set; } = true;

	/// <summary>
	/// Indicates whether the invite has already been used at least once.
	/// This is updated when the invite is redeemed.
	/// </summary>
	public bool IsUsed { get; set; } = false;

	/// <summary>
	/// Number of times the invite has been used. Useful when <see cref="OneTimeUse"/> is false.
	/// </summary>
	public int TimesUsed { get; set; } = 0;

	/// <summary>
	/// Collection of group memberships that were created using this invite.
	/// </summary>
	public List<GroupMember> CreatedMemberships { get; set; } = new List<GroupMember>();

	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		if (InvitedUserId != null && !OneTimeUse)
		{
			yield return new ValidationResult("Invites for specific users cannot be multi-use.", new[] { nameof(InvitedUserId), nameof(OneTimeUse) });
		}

		if (InvitedUserId != null && InvitedUserId == CreatedById)
		{
			yield return new ValidationResult("Users cannot invite themselves.", new[] { nameof(InvitedUserId), nameof(CreatedById) });
		}

		if (OneTimeUse && TimesUsed > 1)
		{
			yield return new ValidationResult("One-time use invites cannot have been used more than once.", new[] { nameof(TimesUsed) });
		}

		if (ExpiresAt != null && ExpiresAt <= CreatedAt)
		{
			yield return new ValidationResult("Expiration date must be after the creation date.", new[] { nameof(ExpiresAt) });
		}
	}
}