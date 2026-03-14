using CptcEvents.Data;
using CptcEvents.Models;
using Microsoft.EntityFrameworkCore;

namespace CptcEvents.Services;

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

        // UC3 A1: Direct joins to RequiresInvite groups are not permitted; users must use an invite
        if (group.PrivacyLevel == PrivacyLevel.RequiresInvite && role != RoleType.Owner && inviteId == null)
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
