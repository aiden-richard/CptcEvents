using CptcEvents.Data;
using CptcEvents.Models;
using Microsoft.EntityFrameworkCore;

namespace CptcEvents.Services
{
    public interface IGroupService
    {
        // SECTION: Get information about a group or user's membership
        Task<Group?> GetGroupByIdAsync(int id);
        Task<IEnumerable<Group>> GetGroupsForUserAsync(string userId);
        Task<bool> IsUserOwnerAsync(int groupId, string userId);
        Task<bool> IsUserModeratorAsync(int groupId, string userId);
        Task<bool> IsUserMemberAsync(int groupId, string userId);

        // SECTION: Create, update, or delete groups
        Task<Group> CreateGroupAsync(Group group);
        Task<Group?> UpdateGroupAsync(Group group, string requestingUserId); 
        Task DeleteGroupAsync(int groupId);

        // SECTION: Create, update, or delete memberships
        Task<GroupMember?> AddUserToGroupAsync(int groupId, string userId, RoleType role, int? inviteId);
        Task<GroupMember?> UpdateUserRoleAsync(int groupId, string userId, RoleType newRole);
        Task RemoveUserFromGroupAsync(int groupId, string userId);
    }

    public class GroupService : IGroupService
    {
        private readonly ApplicationDbContext _context;

        public GroupService(ApplicationDbContext context)
        {
            _context = context;
        }

        // SECTION: Get information about a group and users' membership

        public async Task<Group?> GetGroupByIdAsync(int id)
        {
            return await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<IEnumerable<Group>> GetGroupsForUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return Enumerable.Empty<Group>();

            return await _context.Groups
                .Where(g => g.Members.Any(m => m.UserId == userId))
                .ToListAsync();
        }

        public async Task<bool> IsUserOwnerAsync(int groupId, string userId)
        {
            return await _context.GroupMemberships
                .AnyAsync(m => m.GroupId == groupId && m.UserId == userId && m.Role == RoleType.Owner);
        }

        public async Task<bool> IsUserModeratorAsync(int groupId, string userId)
        {
            return await _context.GroupMemberships
                .AnyAsync(m => m.GroupId == groupId && m.UserId == userId && (m.Role == RoleType.Moderator || m.Role == RoleType.Owner));
        }

        public async Task<bool> IsUserMemberAsync(int groupId, string userId)
        {
            return await _context.GroupMemberships
                .AnyAsync(m => m.GroupId == groupId && m.UserId == userId);
        }


        // SECTION: Create, update, and delete groups

        public async Task<Group> CreateGroupAsync(Group group)
        {
            // Run within the same DbContext and save changes once to ensure consistency.
            _context.Groups.Add(group);
            await AddUserToGroupAsync(group.Id, group.OwnerId, RoleType.Owner, null);

            return group;
        }

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
    }
}
