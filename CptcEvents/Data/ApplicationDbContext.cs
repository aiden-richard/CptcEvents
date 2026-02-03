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
        public DbSet<InstructorCode> InstructorCodes { get; set; } = default!;
        public DbSet<EventRsvp> EventRsvps { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure NO ACTION for GroupMember -> User to prevent cascade cycles
            builder.Entity<GroupMember>()
                .HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure cascade delete for Group -> GroupMember relationship
            builder.Entity<GroupMember>()
                .HasOne(m => m.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(m => m.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure NO ACTION for Event -> User to prevent cascade cycles
            builder.Entity<Event>()
                .HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure cascade delete for Group -> Event relationship
            builder.Entity<Event>()
                .HasOne(e => e.Group)
                .WithMany()
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure NO ACTION for GroupInvite -> User relationships to prevent cascade cycles
            builder.Entity<GroupInvite>()
                .HasOne(i => i.CreatedBy)
                .WithMany()
                .HasForeignKey(i => i.CreatedById)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure NO ACTION for GroupInvite -> InvitedUser relationship to prevent cascade cycles
            builder.Entity<GroupInvite>()
                .HasOne(i => i.InvitedUser)
                .WithMany()
                .HasForeignKey(i => i.InvitedUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure cascade delete for Group -> GroupInvite relationship
            builder.Entity<GroupInvite>()
                .HasOne(i => i.Group)
                .WithMany()
                .HasForeignKey(i => i.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure NO ACTION for EventRsvp -> User to prevent cascade cycles
            builder.Entity<EventRsvp>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure cascade delete for Event -> EventRsvp relationship
            builder.Entity<EventRsvp>()
                .HasOne(r => r.Event)
                .WithMany()
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create unique index to prevent duplicate RSVPs for same user/event combination
            builder.Entity<EventRsvp>()
                .HasIndex(r => new { r.EventId, r.UserId })
                .IsUnique();
        }

    }
}
