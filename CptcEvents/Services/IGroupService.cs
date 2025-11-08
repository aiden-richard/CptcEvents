using CptcEvents.Data;
using CptcEvents.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CptcEvents.Services
{
    public interface IGroupService
    {
        Task<IEnumerable<Group>> GetGroupsForUserAsync(string userId);
        Task<Group> AddGroupAsync(Group group);
        Task AddMemberToGroupAsync(int groupId, string userId);
    }

    public class GroupService : IGroupService
    {
        private readonly ApplicationDbContext _context;

        public GroupService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Group>> GetGroupsForUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return Enumerable.Empty<Group>();

            return await _context.Group
                .Where(g => g.Members.Any(m => m.Id == userId))
                .ToListAsync();
        }

        public async Task<Group> AddGroupAsync(Group group)
        {
            _context.Group.Add(group);
            await _context.SaveChangesAsync();
            return group;
        }

        public async Task AddMemberToGroupAsync(int groupId, string userId)
        {
            var group = await _context.Group.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null) return;

            // Try to find user by id
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            // Prevent duplicate membership
            if (!group.Members.Any(m => m.Id == userId))
            {
                group.Members.Add(user as ApplicationUser ?? throw new InvalidOperationException("User is not ApplicationUser"));
                await _context.SaveChangesAsync();
            }
        }
    }
}
