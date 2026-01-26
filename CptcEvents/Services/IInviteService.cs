using CptcEvents.Data;
using CptcEvents.Models;
using Microsoft.EntityFrameworkCore;

namespace CptcEvents.Services;

/// <summary>
/// Service contract for group invite-related operations including creation, retrieval, and redemption.
/// </summary>
public interface IInviteService
{
	/// <summary>
	/// Retrieves a group invite by its identifier, including related group data.
	/// </summary>
	/// <param name="id">Invite id.</param>
	/// <returns>The invite or <c>null</c> if not found.</returns>
    public Task<GroupInvite?> GetInviteAsync(int id);

	/// <summary>
	/// Gets all invites created by a specific user, including group and creator information.
	/// </summary>
	/// <param name="userId">The user's Id.</param>
	/// <returns>List of invites created by the user.</returns>
	public Task<List<GroupInvite>> GetInvitesCreatedByUserAsync(string userId);

	/// <summary>
	/// Retrieves all invites for a given group, including creator and invited user data.
	/// </summary>
	/// <param name="groupId">The group id.</param>
	/// <returns>List of invites belonging to the group.</returns>
	public Task<List<GroupInvite>> GetInvitesForGroupAsync(int groupId);

	/// <summary>
	/// Creates a new group invite and persists it to the database.
	/// </summary>
	/// <param name="invite">The invite entity to create.</param>
	/// <returns>The created invite with generated Id populated.</returns>
	public Task<GroupInvite> CreateInviteAsync(GroupInvite invite);

	/// <summary>
	/// Persists changes to an invite.
	/// </summary>
	/// <param name="invite">The invite entity to update.</param>
	/// <returns>The updated invite.</returns>
	public Task<GroupInvite> UpdateInviteAsync(GroupInvite invite);

	/// <summary>
	/// Deletes an invite by id if it exists.
	/// </summary>
	/// <param name="inviteId">The invite id.</param>
	public Task DeleteInviteAsync(int inviteId);

	/// <summary>
	/// Attempts to redeem an invite for a user, creating a group membership if valid.
	/// Handles validation for expiration, usage limits, and duplicate memberships.
	/// </summary>
	/// <param name="inviteId">The invite id to redeem.</param>
	/// <param name="userId">The user attempting to redeem the invite.</param>
	/// <returns>The created group membership or <c>null</c> if redemption failed.</returns>
	public Task<GroupMember?> RedeemInviteAsync(int inviteId, string userId);

	/// <summary>
	/// Retrieves an invite by its code (case-insensitive), including group information.
	/// </summary>
	/// <param name="code">The invite code to look up.</param>
	/// <returns>The invite or <c>null</c> if not found.</returns>
	public Task<GroupInvite?> GetInviteByCodeAsync(string code);

	/// <summary>
	/// Checks whether an invite code is already in use (case-insensitive).
	/// </summary>
	/// <param name="code">The invite code to check.</param>
	/// <returns><c>true</c> if the code exists; otherwise <c>false</c>.</returns>
	Task<bool> InviteCodeInUseAsync(string code);

	/// <summary>
	/// Generates a unique invite code of the specified length that is not already in use.
	/// </summary>
	/// <param name="length">The desired length of the invite code.</param>
	/// <returns>A unique invite code string.</returns>
	public Task<string> GenerateUniqueInviteCodeAsync(int length = 8);

	/// <summary>
	/// Validates all authorization and business rules for creating a group invite.
	/// Checks group existence, privacy level permissions, invited user validation, and expiry rules.
	/// </summary>
	/// <param name="currentUserId">The ID of the user attempting to create the invite.</param>
	/// <param name="invite">The invite view model with creation details.</param>
	/// <returns>Validation result with errors, authorized status, and validated data.</returns>
	public Task<InviteValidationResult> ValidateCreateInviteAsync(string currentUserId, GroupInviteViewModel invite);

	/// <summary>
	/// Validates authorization and business rules for editing an existing invite.
	/// </summary>
	/// <param name="currentUserId">The ID of the user attempting to edit.</param>
	/// <param name="invite">The existing invite entity.</param>
	/// <param name="editModel">The edit view model with updated fields.</param>
	/// <returns>Validation result with errors, authorized status, and validated data.</returns>
	public Task<InviteValidationResult> ValidateUpdateInviteAsync(string currentUserId, GroupInvite invite, GroupInviteEditViewModel editModel);
}

/// <summary>
/// Concrete implementation of <see cref="IInviteService"/> for managing group invites.
/// </summary>
public class InviteService : IInviteService
{
    private readonly ApplicationDbContext _context;

	private readonly IGroupService _groupService;

	private readonly Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> _userManager;

	/// <summary>
	/// Creates a new instance of <see cref="InviteService"/>.
	/// </summary>
	/// <param name="context">Application DbContext (injected).</param>
	/// <param name="userManager">User manager for identity operations (injected).</param>
	public InviteService(ApplicationDbContext context, IGroupService groupService, Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager)
	{
		_context = context;
		_groupService = groupService;
		_userManager = userManager;
	}

	/// <inheritdoc/>
	public async Task<GroupInvite?> GetInviteAsync(int id)
	{
		// Include the Group navigation property so views can access GroupInvite.Group safely
		return await _context.GroupInvites
			.Include(i => i.Group)
			.Include(i => i.InvitedUser)
			.FirstOrDefaultAsync(i => i.Id == id);
	}

	/// <inheritdoc/>
	public async Task<List<GroupInvite>> GetInvitesCreatedByUserAsync(string userId)
	{
		if (string.IsNullOrWhiteSpace(userId)) return new List<GroupInvite>();
		return await _context.GroupInvites
			.Include(i => i.Group)
			.Include(i => i.CreatedBy)
			.Where(i => i.CreatedById == userId)
			.ToListAsync();
	}

	/// <inheritdoc/>
	public async Task<List<GroupInvite>> GetInvitesForGroupAsync(int groupId)
	{
		return await _context.GroupInvites
			.Include(i => i.Group)
			.Include(i => i.CreatedBy)
			.Include(i => i.InvitedUser)
			.Where(i => i.GroupId == groupId)
			.OrderByDescending(i => i.CreatedAt)
			.ToListAsync();
	}

	/// <inheritdoc/>
	public async Task<GroupInvite> CreateInviteAsync(GroupInvite invite)
	{
		_context.GroupInvites.Add(invite);
		await _context.SaveChangesAsync();
		return invite;
	}

	/// <inheritdoc/>
	public async Task<GroupInvite> UpdateInviteAsync(GroupInvite invite)
	{
		_context.GroupInvites.Update(invite);
		await _context.SaveChangesAsync();
		return invite;
	}

	/// <inheritdoc/>
	public async Task DeleteInviteAsync(int inviteId)
	{
		GroupInvite? invite = await _context.GroupInvites.FirstOrDefaultAsync(i => i.Id == inviteId);
		if (invite == null)
		{
			return;
		}

		_context.GroupInvites.Remove(invite);
		await _context.SaveChangesAsync();
	}

	/// <inheritdoc/>
	public async Task<GroupMember?> RedeemInviteAsync(int inviteId, string userId)
	{
		GroupInvite? invite = await _context.GroupInvites
			.Include(i => i.Group)
			.FirstOrDefaultAsync(i => i.Id == inviteId);

		if (invite == null)
		{
			return null;
		}

		// Reject if expired
		if (invite.IsExpired)
        {
            return null;
        }

		GroupMember? newMember = await _groupService.AddUserToGroupAsync(invite.GroupId, userId, RoleType.Member, invite.Id);
		if (newMember == null)
		{
			return null;
		}

		// Update invite usage metadata
		invite.TimesUsed += 1;

		await _context.SaveChangesAsync();
		return newMember;
	}

	/// <inheritdoc/>
	public async Task<GroupInvite?> GetInviteByCodeAsync(string code)
	{
		if (string.IsNullOrWhiteSpace(code)) return null;
		string normalized = code.Trim().ToUpper();
		return await _context.GroupInvites
			.Include(i => i.Group)
			.FirstOrDefaultAsync(i => i.InviteCode.ToUpper() == normalized);
	}

	/// <inheritdoc/>
	public async Task<bool> InviteCodeInUseAsync(string code)
	{
		if (string.IsNullOrWhiteSpace(code)) return false;
		string normalized = code.Trim().ToUpper();
		return await _context.GroupInvites.AnyAsync(i => i.InviteCode.ToUpper() == normalized);
	}

	/// <inheritdoc/>
	public async Task<string> GenerateUniqueInviteCodeAsync(int length = 8)
    {
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		const int maxRetries = 10;
		for (int attempt = 0; attempt < maxRetries; attempt++)
		{
			char[] result = new char[length];
			for (int i = 0; i < length; i++)
			{
				int idx = System.Security.Cryptography.RandomNumberGenerator.GetInt32(chars.Length);
				result[i] = chars[idx];
			}

			string code = new string(result);
			if (!await InviteCodeInUseAsync(code))
			{
				return code;
			}
			// else, try again
		}
		throw new System.Exception($"Failed to generate a unique invite code after {maxRetries} retries.");
    }

	/// <inheritdoc/>
	public async Task<InviteValidationResult> ValidateCreateInviteAsync(string currentUserId, GroupInviteViewModel invite)
	{
		InviteValidationResult result = new InviteValidationResult();

		if (string.IsNullOrWhiteSpace(currentUserId))
		{
			result.IsValid = false;
			result.Unauthorized = true;
			return result;
		}

		Group? group = await _groupService.GetGroupByIdAsync(invite.GroupId);
		if (group == null)
		{
			result.IsValid = false;
			result.NotFound = true;
			return result;
		}

		// Check for owner-only privacy level
		if (group.PrivacyLevel == PrivacyLevel.OwnerInvitePrivate && group.OwnerId != currentUserId)
		{
			result.IsValid = false;
			result.Unauthorized = true;
			return result;
		}

		// Resolve invited username, if present
		if (!string.IsNullOrWhiteSpace(invite.Username))
		{
			string username = invite.Username.Trim();
			ApplicationUser? invitedUser = await _userManager.FindByNameAsync(username);
			if (invitedUser == null)
			{
				result.IsValid = false;
				result.FieldErrors["Username"] = "The specified user does not exist.";
				return result;
			}
			if (invitedUser.Id == currentUserId)
			{
				result.IsValid = false;
				result.FieldErrors["Username"] = "You cannot invite yourself.";
				return result;
			}
			if (invitedUser != null && invite.OneTimeUse == false)
			{
				result.IsValid = false;
				result.FieldErrors["OneTimeUse"] = "Invites for specific users cannot be multi-use.";
				return result;
			}
			result.InvitedUserId = invitedUser?.Id;
		}

		// Validate expiry using local time input converted to UTC
		DateTime? expiresAtUtc = null;
		if (invite.Expires)
		{
			if (!invite.ExpiresAt.HasValue)
			{
				result.IsValid = false;
				result.FieldErrors["ExpiresAt"] = "Expiration date/time must be provided when expiry is enabled.";
			}
			else
			{
				DateTime expiresAtLocal = DateTime.SpecifyKind(invite.ExpiresAt.Value, DateTimeKind.Local);
				expiresAtUtc = expiresAtLocal.ToUniversalTime();
				if (expiresAtUtc <= DateTime.UtcNow)
				{
					result.IsValid = false;
					result.FieldErrors["ExpiresAt"] = "Expiration must be a future date/time.";
				}
			}
		}

		// If no expiry selected, keep null
		result.ValidatedExpiresAt = invite.Expires ? expiresAtUtc : null;

		return result;
	}

	/// <inheritdoc/>
	public async Task<InviteValidationResult> ValidateUpdateInviteAsync(string currentUserId, GroupInvite invite, GroupInviteEditViewModel editModel)
	{
		InviteValidationResult result = new InviteValidationResult();

		if (string.IsNullOrWhiteSpace(currentUserId))
		{
			result.IsValid = false;
			result.Unauthorized = true;
			return result;
		}

		if (invite == null)
		{
			result.IsValid = false;
			result.NotFound = true;
			return result;
		}

		Group? group = invite.Group ?? await _groupService.GetGroupByIdAsync(invite.GroupId);
		if (group == null)
		{
			result.IsValid = false;
			result.NotFound = true;
			return result;
		}

		if (group.PrivacyLevel == PrivacyLevel.OwnerInvitePrivate && group.OwnerId != currentUserId)
		{
			result.IsValid = false;
			result.Unauthorized = true;
			return result;
		}

		DateTime? expiresAtUtc = null;
		if (editModel.Expires)
		{
			if (!editModel.ExpiresAt.HasValue)
			{
				result.IsValid = false;
				result.FieldErrors["ExpiresAt"] = "Expiration date/time must be provided when expiry is enabled.";
			}
			else
			{
				expiresAtUtc = editModel.ExpiresAtUtc;
				if (expiresAtUtc.HasValue && expiresAtUtc <= DateTime.UtcNow)
				{
					result.IsValid = false;
					result.FieldErrors["ExpiresAt"] = "Expiration must be a future date/time.";
				}
			}
		}

		if (invite.InvitedUserId != null && editModel.OneTimeUse == false)
		{
			result.IsValid = false;
			result.FieldErrors["OneTimeUse"] = "Invites for specific users must be one-time use.";
		}

		result.ValidatedExpiresAt = editModel.Expires ? expiresAtUtc : null;
		result.InvitedUserId = invite.InvitedUserId;

		return result;
	}
}