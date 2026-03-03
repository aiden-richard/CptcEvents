using CptcEvents.Models;

namespace CptcEvents.Services;

/// <summary>
/// Service interface for managing groups, memberships, and user roles within groups.
/// Provides methods to retrieve group information, manage group lifecycle, and control user membership and permissions.
/// </summary>
public interface IGroupService
{
    /// <summary>
    /// Retrieves a group by its unique identifier, including all members and their user information.
    /// </summary>
    /// <param name="id">The ID of the group to retrieve.</param>
    /// <returns>The group if found; otherwise, null.</returns>
    Task<Group?> GetGroupByIdAsync(int id);

    /// <summary>
    /// Retrieves all groups that a specific user is a member of.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A collection of groups the user belongs to, or empty if user ID is null/empty.</returns>
    Task<IEnumerable<Group>> GetGroupsForUserAsync(string userId);

    /// <summary>
    /// Retrieves all groups in the system.
    /// </summary>
    /// <returns>A collection of all groups.</returns>
    Task<IEnumerable<Group>> GetAllGroupsAsync();

    /// <summary>
    /// Retrieves groups visible to a user, considering admin privileges.
    /// Admins can see all groups regardless of membership.
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve groups for.</param>
    /// <param name="isAdmin">Whether the user has admin privileges.</param>
    /// <returns>A collection of groups. All groups for admins, or user's groups for non-admins.</returns>
    Task<IEnumerable<Group>> GetGroupsForUserAsync(string userId, bool isAdmin);

    /// <summary>
    /// Determines whether a user is the owner of a specific group.
    /// </summary>
    /// <param name="groupId">The ID of the group.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>True if the user is the owner; otherwise, false.</returns>
    Task<bool> IsUserOwnerAsync(int groupId, string userId);

    /// <summary>
    /// Determines whether a user is a moderator or higher in a specific group.
    /// </summary>
    /// <param name="groupId">The ID of the group.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>True if the user is a moderator or owner; otherwise, false.</returns>
    Task<bool> IsUserModeratorAsync(int groupId, string userId);

    /// <summary>
    /// Determines whether a user is a member of a specific group.
    /// </summary>
    /// <param name="groupId">The ID of the group.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>True if the user is a member; otherwise, false.</returns>
    Task<bool> IsUserMemberAsync(int groupId, string userId);

    /// <summary>
    /// Creates a new group and automatically adds the group owner as a member with owner role.
    /// </summary>
    /// <param name="group">The group entity to create. The OwnerId must be set.</param>
    /// <returns>The created group with Id populated.</returns>
    Task<Group> CreateGroupAsync(Group group);

    /// <summary>
    /// Updates an existing group's basic information. Moderators can update name and description;
    /// only owners can update privacy level.
    /// </summary>
    /// <param name="group">The group with updated values.</param>
    /// <param name="requestingUserId">The ID of the user attempting the update.</param>
    /// <returns>The updated group if successful and authorized; otherwise, null.</returns>
    Task<Group?> UpdateGroupAsync(Group group, string requestingUserId);

    /// <summary>
    /// Deletes a group and all associated members, events, and invites (cascade delete).
    /// </summary>
    /// <param name="groupId">The ID of the group to delete.</param>
    Task DeleteGroupAsync(int groupId);

    /// <summary>
    /// Adds a user to a group with the specified role.
    /// </summary>
    /// <param name="groupId">The ID of the group.</param>
    /// <param name="userId">The ID of the user to add.</param>
    /// <param name="role">The role to assign to the user.</param>
    /// <param name="inviteId">The ID of the invite used to join, if applicable.</param>
    /// <returns>The created membership if successful; otherwise, null (e.g., user already member, invalid group, invalid owner assignment).</returns>
    Task<GroupMember?> AddUserToGroupAsync(int groupId, string userId, RoleType role, int? inviteId = null);

    /// <summary>
    /// Updates a user's role within a group. Cannot promote to or demote from owner role.
    /// </summary>
    /// <param name="groupId">The ID of the group.</param>
    /// <param name="userId">The ID of the user whose role is being updated.</param>
    /// <param name="newRole">The new role to assign.</param>
    /// <returns>The updated membership if successful; otherwise, null (e.g., membership not found, attempting to change owner role).</returns>
    Task<GroupMember?> UpdateUserRoleAsync(int groupId, string userId, RoleType newRole);

    /// <summary>
    /// Removes a user from a group by deleting their membership record.
    /// </summary>
    /// <param name="groupId">The ID of the group.</param>
    /// <param name="userId">The ID of the user to remove.</param>
    Task RemoveUserFromGroupAsync(int groupId, string userId);
}
