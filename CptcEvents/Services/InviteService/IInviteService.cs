using CptcEvents.Models;

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
