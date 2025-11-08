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

        public DbSet<Models.Event> Events { get; set; } = null!;
        public DbSet<CptcEvents.Models.Group> Group { get; set; } = default!;
    }
}
