using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CptcEvents.Models;
using System;

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

            // Seed data
            SeedData(builder);
        }

        private void SeedData(ModelBuilder builder)
        {
            // Seed roles
            var adminRole = new IdentityRole { Id = "admin-role-id", Name = "Admin", NormalizedName = "ADMIN" };
            var staffRole = new IdentityRole { Id = "staff-role-id", Name = "Staff", NormalizedName = "STAFF" };
            var studentRole = new IdentityRole { Id = "student-role-id", Name = "Student", NormalizedName = "STUDENT" };

            builder.Entity<IdentityRole>().HasData(adminRole, staffRole, studentRole);

            // Seed default admin user
            var passwordHasher = new PasswordHasher<ApplicationUser>();
            var adminUser = new ApplicationUser
            {
                Id = "admin-user-id",
                UserName = "admin@cptc.edu",
                NormalizedUserName = "ADMIN@CPTC.EDU",
                Email = "admin@cptc.edu",
                NormalizedEmail = "ADMIN@CPTC.EDU",
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            };
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin123!");

            builder.Entity<ApplicationUser>().HasData(adminUser);

            // Assign admin role to admin user
            builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                RoleId = adminRole.Id,
                UserId = adminUser.Id
            });

            // Seed default group
            var defaultGroup = new Group
            {
                Id = 1,
                Name = "Cptc Dates",
                Description = "Default group for CPTC events and announcements",
                OwnerId = adminUser.Id,
                CreatedAt = new DateTime(2025, 12, 10, 0, 0, 0, DateTimeKind.Utc),
                PrivacyLevel = PrivacyLevel.Public,
                Color = "#502a7f"
            };

            builder.Entity<Group>().HasData(defaultGroup);

            // Make admin user a member of the default group
            var adminMembership = new GroupMember
            {
                Id = 1,
                GroupId = defaultGroup.Id,
                UserId = adminUser.Id,
                Role = RoleType.Owner,
                JoinedAt = new DateTime(2025, 12, 10, 0, 0, 0, DateTimeKind.Utc)
            };

            builder.Entity<GroupMember>().HasData(adminMembership);

            // Seed comprehensive CPTC academic calendar events
            var sampleEvents = new[]
            {
                // Summer 2025 Quarter Events
                new Event { Id = 1, Title = "Summer 2025 - Priority Registration", Description = "Priority registration for Summer 2025 quarter", DateOfEvent = new DateOnly(2025, 5, 19), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 2, Title = "Summer 2025 - Continuing Student Registration", Description = "Registration period for continuing students (May 20-23)", DateOfEvent = new DateOnly(2025, 5, 20), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 3, Title = "Summer 2025 - Open Registration", Description = "Open registration for all admitted students (May 27 - July 2)", DateOfEvent = new DateOnly(2025, 5, 27), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 4, Title = "Summer 2025 - Tuition & Fees Deadline", Description = "Deadline for tuition and fees payment", DateOfEvent = new DateOnly(2025, 6, 17), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 5, Title = "Summer 2025 - First Day of Quarter", Description = "First day of Summer 2025 quarter", DateOfEvent = new DateOnly(2025, 7, 1), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 6, Title = "Summer 2025 - Last Day to Drop (100% Refund)", Description = "Last day to drop with 100% refund", DateOfEvent = new DateOnly(2025, 7, 8), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 7, Title = "Summer 2025 - Last Day to Withdraw (50% Refund)", Description = "Last day to withdraw with 50% refund", DateOfEvent = new DateOnly(2025, 7, 29), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 8, Title = "Summer 2025 - Last Day to Withdraw (W Grade)", Description = "Last day to withdraw with W grade", DateOfEvent = new DateOnly(2025, 8, 19), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 9, Title = "Summer 2025 - Graduation Application Deadline", Description = "Deadline for graduation application", DateOfEvent = new DateOnly(2025, 7, 25), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 10, Title = "Summer 2025 - Last Day of Quarter", Description = "Last day of Summer 2025 quarter", DateOfEvent = new DateOnly(2025, 9, 2), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 11, Title = "Summer 2025 - Official Grades Posted", Description = "Official grades on transcript (ccLink)", DateOfEvent = new DateOnly(2025, 9, 8), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },

                // Fall 2025 Quarter Events
                new Event { Id = 12, Title = "Fall 2025 - Priority Registration", Description = "Priority registration for Fall 2025 quarter", DateOfEvent = new DateOnly(2025, 5, 19), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 13, Title = "Fall 2025 - Continuing Student Registration", Description = "Registration period for continuing students (May 20-23)", DateOfEvent = new DateOnly(2025, 5, 20), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 14, Title = "Fall 2025 - Open Registration", Description = "Open registration for all admitted students (May 27 - Sept 30)", DateOfEvent = new DateOnly(2025, 5, 27), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 15, Title = "Fall 2025 - Tuition & Fees Deadline", Description = "Deadline for tuition and fees payment", DateOfEvent = new DateOnly(2025, 9, 15), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 16, Title = "Fall 2025 - First Day of Quarter", Description = "First day of Fall 2025 quarter", DateOfEvent = new DateOnly(2025, 9, 29), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 17, Title = "Fall 2025 - Last Day to Drop (100% Refund)", Description = "Last day to drop with 100% refund", DateOfEvent = new DateOnly(2025, 10, 3), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 18, Title = "Fall 2025 - Last Day to Withdraw (50% Refund)", Description = "Last day to withdraw with 50% refund", DateOfEvent = new DateOnly(2025, 10, 28), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 19, Title = "Fall 2025 - Last Day to Withdraw (W Grade)", Description = "Last day to withdraw with W grade", DateOfEvent = new DateOnly(2025, 11, 19), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 20, Title = "Fall 2025 - Graduation Application Deadline", Description = "Deadline for graduation application", DateOfEvent = new DateOnly(2025, 10, 24), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 21, Title = "Fall 2025 - Last Day of Quarter", Description = "Last day of Fall 2025 quarter", DateOfEvent = new DateOnly(2025, 12, 12), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 22, Title = "Fall 2025 - Official Grades Posted", Description = "Official grades on transcript (ccLink)", DateOfEvent = new DateOnly(2025, 12, 18), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },

                // Winter 2026 Quarter Events
                new Event { Id = 23, Title = "Winter 2026 - Priority Registration", Description = "Priority registration for Winter 2026 quarter", DateOfEvent = new DateOnly(2025, 11, 17), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 24, Title = "Winter 2026 - Continuing Student Registration", Description = "Registration period for continuing students (Nov 18-21)", DateOfEvent = new DateOnly(2025, 11, 18), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 25, Title = "Winter 2026 - Open Registration", Description = "Open registration for all admitted students (Nov 24 - Jan 6)", DateOfEvent = new DateOnly(2025, 11, 24), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 26, Title = "Winter 2026 - Tuition & Fees Deadline", Description = "Deadline for tuition and fees payment", DateOfEvent = new DateOnly(2025, 12, 18), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 27, Title = "Winter 2026 - First Day of Quarter", Description = "First day of Winter 2026 quarter", DateOfEvent = new DateOnly(2026, 1, 5), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 28, Title = "Winter 2026 - Last Day to Drop (100% Refund)", Description = "Last day to drop with 100% refund", DateOfEvent = new DateOnly(2026, 1, 9), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 29, Title = "Winter 2026 - Last Day to Withdraw (50% Refund)", Description = "Last day to withdraw with 50% refund", DateOfEvent = new DateOnly(2026, 2, 2), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 30, Title = "Winter 2026 - Last Day to Withdraw (W Grade)", Description = "Last day to withdraw with W grade", DateOfEvent = new DateOnly(2026, 2, 24), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 31, Title = "Winter 2026 - Graduation Application Deadline", Description = "Deadline for graduation application", DateOfEvent = new DateOnly(2026, 1, 30), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 32, Title = "Winter 2026 - Last Day of Quarter", Description = "Last day of Winter 2026 quarter", DateOfEvent = new DateOnly(2026, 3, 18), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 33, Title = "Winter 2026 - Official Grades Posted", Description = "Official grades on transcript (ccLink)", DateOfEvent = new DateOnly(2026, 3, 24), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },

                // Spring 2026 Quarter Events
                new Event { Id = 34, Title = "Spring 2026 - Priority Registration", Description = "Priority registration for Spring 2026 quarter", DateOfEvent = new DateOnly(2026, 2, 2), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 35, Title = "Spring 2026 - Continuing Student Registration", Description = "Registration period for continuing students (Feb 3-6)", DateOfEvent = new DateOnly(2026, 2, 3), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 36, Title = "Spring 2026 - Open Registration", Description = "Open registration for all admitted students (Feb 9 - Mar 31)", DateOfEvent = new DateOnly(2026, 2, 9), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 37, Title = "Spring 2026 - Tuition & Fees Deadline", Description = "Deadline for tuition and fees payment", DateOfEvent = new DateOnly(2026, 3, 16), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 38, Title = "Spring 2026 - First Day of Quarter", Description = "First day of Spring 2026 quarter", DateOfEvent = new DateOnly(2026, 3, 30), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 39, Title = "Spring 2026 - Last Day to Drop (100% Refund)", Description = "Last day to drop with 100% refund", DateOfEvent = new DateOnly(2026, 4, 3), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 40, Title = "Spring 2026 - Last Day to Withdraw (50% Refund)", Description = "Last day to withdraw with 50% refund", DateOfEvent = new DateOnly(2026, 4, 24), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 41, Title = "Spring 2026 - Last Day to Withdraw (W Grade)", Description = "Last day to withdraw with W grade", DateOfEvent = new DateOnly(2026, 5, 18), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 42, Title = "Spring 2026 - Graduation Application Deadline", Description = "Deadline for graduation application", DateOfEvent = new DateOnly(2026, 4, 24), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 43, Title = "Spring 2026 - Last Day of Quarter", Description = "Last day of Spring 2026 quarter", DateOfEvent = new DateOnly(2026, 6, 9), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 44, Title = "Spring 2026 - Official Grades Posted", Description = "Official grades on transcript (ccLink)", DateOfEvent = new DateOnly(2026, 6, 15), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },

                // Financial Aid Deadlines
                new Event { Id = 45, Title = "Summer 2025 - Financial Aid Application Deadline", Description = "Deadline for CPTC Financial Aid application process", DateOfEvent = new DateOnly(2025, 5, 23), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 46, Title = "Fall 2025 - Financial Aid Application Deadline", Description = "Deadline for CPTC Financial Aid application process", DateOfEvent = new DateOnly(2025, 6, 27), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 47, Title = "Winter 2026 - Financial Aid Application Deadline", Description = "Deadline for CPTC Financial Aid application process", DateOfEvent = new DateOnly(2025, 11, 14), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id },
                new Event { Id = 48, Title = "Spring 2026 - Financial Aid Application Deadline", Description = "Deadline for CPTC Financial Aid application process", DateOfEvent = new DateOnly(2026, 2, 20), IsAllDay = true, IsPublic = true, IsApprovedPublic = true, GroupId = defaultGroup.Id, CreatedByUserId = adminUser.Id }
            };

            builder.Entity<Event>().HasData(sampleEvents);
        }
    }
}
