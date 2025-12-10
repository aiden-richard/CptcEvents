using CptcEvents.Data;
using CptcEvents.Models;
using Microsoft.EntityFrameworkCore;

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

/// <summary>
/// Implementation of <see cref="IGroupService"/> providing group management functionality
/// using Entity Framework Core and the application's database context.
/// </summary>
public class GroupService : IGroupService
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupService"/> class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    public GroupService(ApplicationDbContext context)
    {
        _context = context;
    }

    // SECTION: Get information about a group and users' membership

    /// <inheritdoc/>
    public async Task<Group?> GetGroupByIdAsync(int id)
    {
        return await _context.Groups
            .Include(g => g.Members)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Group>> GetGroupsForUserAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return Enumerable.Empty<Group>();

        return await _context.Groups
            .Where(g => g.Members.Any(m => m.UserId == userId))
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> IsUserOwnerAsync(int groupId, string userId)
    {
        return await _context.GroupMemberships
            .AnyAsync(m => m.GroupId == groupId && m.UserId == userId && m.Role == RoleType.Owner);
    }

    /// <inheritdoc/>
    public async Task<bool> IsUserModeratorAsync(int groupId, string userId)
    {
        return await _context.GroupMemberships
            .AnyAsync(m => m.GroupId == groupId && m.UserId == userId && (m.Role == RoleType.Moderator || m.Role == RoleType.Owner));
    }

    /// <inheritdoc/>
    public async Task<bool> IsUserMemberAsync(int groupId, string userId)
    {
        return await _context.GroupMemberships
            .AnyAsync(m => m.GroupId == groupId && m.UserId == userId);
    }


    // SECTION: Create, update, and delete groups

    /// <inheritdoc/>
    public async Task<Group> CreateGroupAsync(Group group)
    {
        // Add the group and persist it so the database generates an Id
        // before we attempt to add the owner membership that relies on group.Id.
        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        // Now group.Id is populated; add the owner membership.
        await AddUserToGroupAsync(group.Id, group.OwnerId, RoleType.Owner, null);

        return group;
    }

    /// <inheritdoc/>
    public async Task<Group?> UpdateGroupAsync(Group updatedGroup, string requestingUserId)
    {
        Group? existingGroup = await _context.Groups.FirstOrDefaultAsync(g => g.Id == updatedGroup.Id);
        if (existingGroup == null)
        {
            return null;
        }

        // Check if user is at least a moderator to update basic info
        bool isModerator = await IsUserModeratorAsync(updatedGroup.Id, requestingUserId);
        if (!isModerator)
        {
            return null;
        }

        // Update group info (moderators and owners can do this)
        existingGroup.Name = updatedGroup.Name;
        existingGroup.Description = updatedGroup.Description;
        
        // Update higher permission variables if request is from owner
        bool isOwner = await IsUserOwnerAsync(updatedGroup.Id, requestingUserId);
        if (isOwner)
        {
            existingGroup.PrivacyLevel = updatedGroup.PrivacyLevel;
            
            // Note: OwnerId should not be changed here to prevent ownership transfer
        }

        await _context.SaveChangesAsync();
        return existingGroup;
    }

    /// <inheritdoc/>
    public async Task DeleteGroupAsync(int groupId)
    {
        Group? group = await _context.Groups.FindAsync(groupId);

        if (group == null)
        {
            return;
        }

        // EF Core will cascade delete Members, Events, and Invites
        _context.Groups.Remove(group);
        await _context.SaveChangesAsync();
    }


    // SECTION: Add, update, and remove members

    /// <inheritdoc/>
    public async Task<GroupMember?> AddUserToGroupAsync(int groupId, string userId, RoleType role, int? inviteId = null)
    {
        // Get group with members
        Group? group = await _context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
        {
            return null;
        }
        
        // Don't add duplicate membership (fast pre-check)
        if (group.Members.Any(g => g.UserId == userId))
        {
            return null;
        }

        // For a user to be added to a group as an owner, the userId of the request must match
        // the OwnerId of the group which was set on group creation
        if (role == RoleType.Owner && group.OwnerId != userId)
        {
            return null;
        }

        GroupMember newMember = new GroupMember
        {
            InviteId = inviteId,
            GroupId = groupId,
            UserId = userId,
            Role = role
        };

        _context.GroupMemberships.Add(newMember);
        await _context.SaveChangesAsync();
        return newMember;
    }

    /// <inheritdoc/>
    public async Task<GroupMember?> UpdateUserRoleAsync(int groupId, string userId, RoleType newRole)
    {
        // Get the membership record
        GroupMember? membership = await _context.GroupMemberships
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);

        if (membership == null)
        {
            return null;
        }

        // Prevent changing role to Owner through this method
        // Owner role should only be set during group creation
        if (newRole == RoleType.Owner)
        {
            return null;
        }

        // Prevent changing the Owner's role
        if (membership.Role == RoleType.Owner)
        {
            return null;
        }

        membership.Role = newRole;
        await _context.SaveChangesAsync();
        return membership;
    }

    /// <inheritdoc/>
    public async Task RemoveUserFromGroupAsync(int groupId, string userId)
    {
        // Get membership record
        GroupMember? membership = await _context.GroupMemberships
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);
        if (membership == null)
        {
            return;
        }

        _context.GroupMemberships.Remove(membership);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Group>> GetAllGroupsAsync()
    {
        return await _context.Groups
            .Include(g => g.Members)
            .ThenInclude(m => m.User)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Group>> GetGroupsForUserAsync(string userId, bool isAdmin)
    {
        if (string.IsNullOrEmpty(userId)) return Enumerable.Empty<Group>();

        if (isAdmin)
        {
            // Admins can see all groups
            return await GetAllGroupsAsync();
        }
        else
        {
            // Regular users see only groups they're members of
            return await GetGroupsForUserAsync(userId);
        }
    }
}
