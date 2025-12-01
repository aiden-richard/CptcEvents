using CptcEvents.Data;
using CptcEvents.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CptcEvents.Services
{
    public interface IGroupService
    {
        Task<Group?> GetGroupAsync(int id);
        Task<bool> IsUserModeratorAsync(int groupId, string userId);
        Task<IEnumerable<Group>> GetGroupsForUserAsync(string userId);
        Task<Group> CreateGroupAsync(Group group);
    }

    public class GroupService : IGroupService
    {
        private readonly ApplicationDbContext _context;

        public GroupService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Group?> GetGroupAsync(int id)
        {
            return await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<bool> IsUserModeratorAsync(int groupId, string userId)
        {
            return await _context.GroupMemberships
                .AnyAsync(m => m.GroupId == groupId && m.UserId == userId && (m.Role == RoleType.Moderator || m.Role == RoleType.Owner));
        }

        public async Task<IEnumerable<Group>> GetGroupsForUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return Enumerable.Empty<Group>();

            return await _context.Groups
                .Where(g => g.Members.Any(m => m.UserId == userId))
                .ToListAsync();
        }

        public async Task<Group> CreateGroupAsync(Group group)
        {
            // Run within the same DbContext and save changes once to ensure consistency.
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            GroupMember member = new GroupMember
            {
                GroupId = group.Id,
                UserId = group.OwnerId,
                Role = RoleType.Owner
            };
            _context.GroupMemberships.Add(member);

            await _context.SaveChangesAsync();
            return group;
        }
    }
}
