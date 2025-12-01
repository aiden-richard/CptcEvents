using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CptcEvents.Models;

namespace CptcEvents.Data
{
    public class ApplicationDbContext : IdentityDbContext<Models.ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Event> Events { get; set; } = default!;
        public DbSet<Group> Groups { get; set; } = default!;
        public DbSet<GroupMember> GroupMemberships { get; set; } = default!;
        public DbSet<GroupInvite> GroupInvites { get; set; } = default!;
    }
}
