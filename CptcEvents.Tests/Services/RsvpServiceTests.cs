using CptcEvents.Models;
using CptcEvents.Services;
using CptcEvents.Tests.Helpers;

namespace CptcEvents.Tests.Services;

/// <summary>
/// Unit tests for <see cref="RsvpService"/>.
/// Covers UC8 (RSVP to an event).
/// </summary>
public class RsvpServiceTests
{
    /// <summary>
    /// Seeds a group, adds <paramref name="memberId"/> as a Member, and creates a future event
    /// for that group. Most UC8 success-path tests use this helper.
    /// </summary>
    private static async Task<(Event ev, Group group)> SeedGroupEventAndMemberAsync(
        CptcEvents.Data.ApplicationDbContext ctx,
        GroupService groupService,
        string memberId = "user-2")
    {
        var group = new Group { Name = "Group", OwnerId = "owner-1", PrivacyLevel = PrivacyLevel.Public };
        await groupService.CreateGroupAsync(group);
        await groupService.AddUserToGroupAsync(group.Id, memberId, RoleType.Member);

        var ev = new Event
        {
            Title = "Event",
            DateOfEvent = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            GroupId = group.Id,
            CreatedByUserId = "owner-1",
            IsAllDay = true
        };
        ctx.Events.Add(ev);
        await ctx.SaveChangesAsync();
        return (ev, group);
    }

    // UC8: RSVP to an event — success

    [Fact]
    public async Task CreateRsvp_RecordsRsvp()
    {
        // Arrange
        using var ctx = TestDbContextFactory.Create();
        var groupService = new GroupService(ctx);
        var service = new RsvpService(ctx, groupService);
        var (ev, _) = await SeedGroupEventAndMemberAsync(ctx, groupService);

        // Act
        var rsvp = await service.CreateRsvpAsync(ev.Id, "user-2", RsvpStatus.Going);

        // Assert
        Assert.NotNull(rsvp);
        Assert.Equal(ev.Id, rsvp.EventId);
        Assert.Equal("user-2", rsvp.UserId);
        Assert.Equal(RsvpStatus.Going, rsvp.Status);
    }

    [Fact]
    public async Task UpdateRsvp_ChangesStatus()
    {
        // Arrange
        using var ctx = TestDbContextFactory.Create();
        var groupService = new GroupService(ctx);
        var service = new RsvpService(ctx, groupService);
        var (ev, _) = await SeedGroupEventAndMemberAsync(ctx, groupService);
        var rsvp = await service.CreateRsvpAsync(ev.Id, "user-2", RsvpStatus.Going);

        // Act
        var updated = await service.UpdateRsvpAsync(rsvp!.Id, RsvpStatus.NotGoing);

        // Assert
        Assert.NotNull(updated);
        Assert.Equal(RsvpStatus.NotGoing, updated.Status);
    }

    [Fact]
    public async Task UpdateRsvp_NotFound_ReturnsNull()
    {
        // Arrange
        using var ctx = TestDbContextFactory.Create();
        var service = new RsvpService(ctx, new GroupService(ctx));

        // Act
        var updated = await service.UpdateRsvpAsync(9999, RsvpStatus.NotGoing);

        // Assert
        Assert.Null(updated);
    }

    // UC8 alternate paths

    [Fact]
    public async Task CreateRsvp_InvalidStatus_ReturnsNull()
    {
        // Arrange — UC8 A3: submitted status is not a valid RsvpStatus value
        using var ctx = TestDbContextFactory.Create();
        var groupService = new GroupService(ctx);
        var service = new RsvpService(ctx, groupService);
        var (ev, _) = await SeedGroupEventAndMemberAsync(ctx, groupService);

        // Act
        var rsvp = await service.CreateRsvpAsync(ev.Id, "user-2", (RsvpStatus)999);

        // Assert
        Assert.Null(rsvp);
    }

    [Fact]
    public async Task CreateRsvp_UserNotMember_ReturnsNull()
    {
        // Arrange — UC8 A1: user is not a member of the event's group
        using var ctx = TestDbContextFactory.Create();
        var groupService = new GroupService(ctx);
        var service = new RsvpService(ctx, groupService);

        var group = new Group { Name = "Group", OwnerId = "owner-1", PrivacyLevel = PrivacyLevel.Public };
        await groupService.CreateGroupAsync(group);
        // "user-2" is NOT added as a member

        var ev = new Event
        {
            Title = "Event",
            DateOfEvent = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            GroupId = group.Id,
            CreatedByUserId = "owner-1",
            IsAllDay = true
        };
        ctx.Events.Add(ev);
        await ctx.SaveChangesAsync();

        // Act
        var rsvp = await service.CreateRsvpAsync(ev.Id, "user-2", RsvpStatus.Going);

        // Assert
        Assert.Null(rsvp);
    }

    [Fact]
    public async Task CreateRsvp_PastEvent_ReturnsNull()
    {
        // Arrange — UC8 A2: the event has already occurred
        using var ctx = TestDbContextFactory.Create();
        var groupService = new GroupService(ctx);
        var service = new RsvpService(ctx, groupService);

        var group = new Group { Name = "Group", OwnerId = "owner-1", PrivacyLevel = PrivacyLevel.Public };
        await groupService.CreateGroupAsync(group);
        await groupService.AddUserToGroupAsync(group.Id, "user-2", RoleType.Member);

        var ev = new Event
        {
            Title = "Past Event",
            DateOfEvent = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            GroupId = group.Id,
            CreatedByUserId = "owner-1",
            IsAllDay = true
        };
        ctx.Events.Add(ev);
        await ctx.SaveChangesAsync();

        // Act
        var rsvp = await service.CreateRsvpAsync(ev.Id, "user-2", RsvpStatus.Going);

        // Assert
        Assert.Null(rsvp);
    }

    [Fact]
    public async Task CreateRsvp_AlreadyRsvped_ReturnsNull()
    {
        // Arrange — duplicate RSVP guard (service-layer; UC8 A2 past-event check is controller-enforced)
        using var ctx = TestDbContextFactory.Create();
        var groupService = new GroupService(ctx);
        var service = new RsvpService(ctx, groupService);
        var (ev, _) = await SeedGroupEventAndMemberAsync(ctx, groupService);
        await service.CreateRsvpAsync(ev.Id, "user-2", RsvpStatus.Going);

        // Act
        var duplicate = await service.CreateRsvpAsync(ev.Id, "user-2", RsvpStatus.Maybe);

        // Assert
        Assert.Null(duplicate);
    }

    [Fact]
    public async Task CreateRsvp_EventNotFound_ReturnsNull()
    {
        // Arrange — event not found guard (service-layer; UC8 A1 membership check is controller-enforced)
        using var ctx = TestDbContextFactory.Create();
        var service = new RsvpService(ctx, new GroupService(ctx));

        // Act
        var rsvp = await service.CreateRsvpAsync(9999, "user-2", RsvpStatus.Going);

        // Assert
        Assert.Null(rsvp);
    }

    [Fact]
    public async Task DeleteRsvp_ExistingRsvp_ReturnsTrue()
    {
        // Arrange
        using var ctx = TestDbContextFactory.Create();
        var groupService = new GroupService(ctx);
        var service = new RsvpService(ctx, groupService);
        var (ev, _) = await SeedGroupEventAndMemberAsync(ctx, groupService);
        var rsvp = await service.CreateRsvpAsync(ev.Id, "user-2", RsvpStatus.Going);

        // Act
        bool deleted = await service.DeleteRsvpAsync(rsvp!.Id);

        // Assert
        Assert.True(deleted);
    }

    [Fact]
    public async Task DeleteRsvp_NotFound_ReturnsFalse()
    {
        // Arrange
        using var ctx = TestDbContextFactory.Create();
        var service = new RsvpService(ctx, new GroupService(ctx));

        // Act
        bool deleted = await service.DeleteRsvpAsync(9999);

        // Assert
        Assert.False(deleted);
    }
}
