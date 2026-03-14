using System.ComponentModel.DataAnnotations;
using CptcEvents.Models;
using CptcEvents.Services;
using CptcEvents.Tests.Helpers;

namespace CptcEvents.Tests.Services;

/// <summary>
/// Unit tests for <see cref="EventService"/>.
/// Covers UC7 (Create an event) and UC10 (Approve or deny a public event).
/// </summary>
public class EventServiceTests
{
    // UC7: Create an event

    [Fact]
    public async Task CreateEvent_PersistsEvent()
    {
        // Using a fresh DbContext prevents EF Core from reusing cached entities, 
        // ensuring the service actually fetches data from the in-memory store.
        string dbName = Guid.NewGuid().ToString();

        // Arrange — seed User + Group in a setup context
        int groupId;
        using (var setupCtx = TestDbContextFactory.Create(dbName))
        {
            var user = new ApplicationUser
            {
                Id = "123",
                UserName = "bobsmith",
                NormalizedUserName = "BOBSMITH",
                Email = "bob@example.com",
                NormalizedEmail = "BOB@EXAMPLE.COM",
                SecurityStamp = Guid.NewGuid().ToString(),
                FirstName = "Bob",
                LastName = "Smith"
            };
            setupCtx.Users.Add(user);
            await setupCtx.SaveChangesAsync();

            var group = new Group { Name = "Test Group", OwnerId = user.Id };
            setupCtx.Groups.Add(group);
            await setupCtx.SaveChangesAsync();
            groupId = group.Id;
        }

        // Act — service runs in a separate context that reads the seeded data
        using var serviceCtx = TestDbContextFactory.Create(dbName);
        var service = new EventService(serviceCtx);

        var newEvent = new Event
        {
            Title = "Test Event",
            DateOfEvent = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            GroupId = groupId,
            CreatedByUserId = "123",
            IsAllDay = true
        };

        var created = await service.CreateEventAsync(newEvent);

        // Assert
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal("Test Event", created.Title);
        Assert.Equal(groupId, created.GroupId);
    }

    // UC7 A2: Event title is empty — model validation rejects it

    [Fact]
    public void CreateEvent_EmptyTitle_FailsModelValidation()
    {
        // Arrange
        var model = new EventFormViewModel
        {
            Title = "",
            GroupId = 1,
            DateOfEvent = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
            IsAllDay = true
        };

        // Act
        var results = new List<ValidationResult>();
        bool valid = Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);

        // Assert
        Assert.False(valid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(EventFormViewModel.Title)));
    }

    // UC10: Approve or deny a public event

    [Fact]
    public async Task ApproveEvent_SetsApprovedStatus()
    {
        // Arrange
        using var ctx = TestDbContextFactory.Create();
        var service = new EventService(ctx);

        var ev = new Event
        {
            Title = "Pending Event",
            DateOfEvent = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
            GroupId = 1,
            CreatedByUserId = "user-1",
            IsAllDay = true,
            ApprovalStatus = ApprovalStatus.PendingApproval
        };
        ctx.Events.Add(ev);
        await ctx.SaveChangesAsync();

        // Act
        bool result = await service.ApproveEventAsync(ev.Id);

        // Assert
        Assert.True(result);
        Assert.Equal(ApprovalStatus.Approved, ctx.Events.Find(ev.Id)!.ApprovalStatus);
    }

    // UC10 alternate paths

    [Fact]
    public async Task DenyEvent_SetsDeniedStatus()
    {
        // Arrange
        using var ctx = TestDbContextFactory.Create();
        var service = new EventService(ctx);

        var ev = new Event
        {
            Title = "Pending Event",
            DateOfEvent = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
            GroupId = 1,
            CreatedByUserId = "user-1",
            IsAllDay = true,
            ApprovalStatus = ApprovalStatus.PendingApproval
        };
        ctx.Events.Add(ev);
        await ctx.SaveChangesAsync();

        // Act
        bool result = await service.DenyEventAsync(ev.Id);

        // Assert
        Assert.True(result);
        Assert.Equal(ApprovalStatus.Denied, ctx.Events.Find(ev.Id)!.ApprovalStatus);
    }

    [Fact]
    public async Task ApproveEvent_NotPending_ReturnsFalse()
    {
        // Arrange — UC10 A2: event is not in PendingApproval status
        using var ctx = TestDbContextFactory.Create();
        var service = new EventService(ctx);

        var ev = new Event
        {
            Title = "Private Event",
            DateOfEvent = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
            GroupId = 1,
            CreatedByUserId = "user-1",
            IsAllDay = true,
            ApprovalStatus = ApprovalStatus.Private
        };
        ctx.Events.Add(ev);
        await ctx.SaveChangesAsync();

        // Act
        bool result = await service.ApproveEventAsync(ev.Id);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DenyEvent_NotPending_ReturnsFalse()
    {
        // Arrange — UC10 A2: event is not in PendingApproval status
        using var ctx = TestDbContextFactory.Create();
        var service = new EventService(ctx);

        var ev = new Event
        {
            Title = "Already Approved Event",
            DateOfEvent = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3)),
            GroupId = 1,
            CreatedByUserId = "user-1",
            IsAllDay = true,
            ApprovalStatus = ApprovalStatus.Approved
        };
        ctx.Events.Add(ev);
        await ctx.SaveChangesAsync();

        // Act
        bool result = await service.DenyEventAsync(ev.Id);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ApproveEvent_NonExistentEvent_ReturnsFalse()
    {
        // Arrange
        using var ctx = TestDbContextFactory.Create();
        var service = new EventService(ctx);

        // Act
        bool result = await service.ApproveEventAsync(9999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DenyEvent_NonExistentEvent_ReturnsFalse()
    {
        // Arrange
        using var ctx = TestDbContextFactory.Create();
        var service = new EventService(ctx);

        // Act
        bool result = await service.DenyEventAsync(9999);

        // Assert
        Assert.False(result);
    }
}
