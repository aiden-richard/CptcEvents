using System.Security.Claims;
using CptcEvents.Authorization.EventAuthorizationService;
using CptcEvents.Models;
using CptcEvents.Services;
using CptcEvents.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace CptcEvents.Tests.Authorization;

/// <summary>
/// Unit tests for <see cref="EventAuthorizationService"/>.
/// Covers UC9 (Request public event visibility).
/// </summary>
public class EventAuthorizationServiceTests
{
    private static Mock<UserManager<ApplicationUser>> CreateMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();

        // These nulls are required by the UserManager constructor for services that
        // aren't needed for these tests
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static ClaimsPrincipal MakeUser(string userId, params string[] roles)
    {
        // Construct a user identity with the provided ID and set of roles for testing
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    private static EventAuthorizationService CreateService(
        Mock<UserManager<ApplicationUser>> userManager,
        Data.ApplicationDbContext ctx)
    {
        // Build the service with its necessary dependencies injected
        var groupService = new GroupService(ctx);
        var eventService = new EventService(ctx);

        return new EventAuthorizationService(userManager.Object, groupService, eventService);
    }

    // UC9: Request public event visibility

    [Fact]
    public async Task CanMakeEventPublic_StaffUser_Succeeds()
    {
        // Arrange
        using var ctx = TestDbContextFactory.Create();
        var userManager = CreateMockUserManager();
        var service = CreateService(userManager, ctx);

        // Event created by a non-student user
        var creator = new ApplicationUser { Id = "creator-1", UserName = "creator", FirstName = "Test", LastName = "User" };
        userManager.Setup(u => u.FindByIdAsync("creator-1")).ReturnsAsync(creator);
        userManager.Setup(u => u.IsInRoleAsync(creator, "Student")).ReturnsAsync(false);

        var ev = new Event
        {
            Title = "Workshop",
            DateOfEvent = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            GroupId = 1,
            CreatedByUserId = "creator-1",
            IsAllDay = true
        };

        var staffUser = MakeUser("staff-1", "Staff");

        // Act - staff user should be able to make the event public regardless of creator's role
        var result = await service.CanMakeEventPublicAsync(ev, staffUser);

        // Assert
        Assert.True(result.Succeeded);
    }

    // UC9 alternate paths

    [Fact]
    public async Task CanMakeEventPublic_NonStaffUser_Fails()
    {
        // Arrange — UC9 A1: user does not hold the Staff role
        using var ctx = TestDbContextFactory.Create();
        var userManager = CreateMockUserManager();
        var service = CreateService(userManager, ctx);

        // event created by a non-student user
        var creator = new ApplicationUser { Id = "creator-1", UserName = "creator", FirstName = "Test", LastName = "User" };
        userManager.Setup(u => u.FindByIdAsync("creator-1")).ReturnsAsync(creator);
        userManager.Setup(u => u.IsInRoleAsync(creator, "Student")).ReturnsAsync(false);

        var ev = new Event
        {
            Title = "Workshop",
            DateOfEvent = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            GroupId = 1,
            CreatedByUserId = "creator-1",
            IsAllDay = true
        };

        var regularUser = MakeUser("user-1");  // no roles, non staff

        // Act
        var result = await service.CanMakeEventPublicAsync(ev, regularUser);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task CanMakeEventPublic_AlreadyPending_Fails()
    {
        // Arrange — UC9 A3: event is already in PendingApproval status
        using var ctx = TestDbContextFactory.Create();
        var userManager = CreateMockUserManager();
        var service = CreateService(userManager, ctx);

        var ev = new Event
        {
            Title = "Pending Event",
            DateOfEvent = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            GroupId = 1,
            CreatedByUserId = "creator-1",
            IsAllDay = true,
            ApprovalStatus = ApprovalStatus.PendingApproval
        };

        var staffUser = MakeUser("staff-1", "Staff");

        // Act
        var result = await service.CanMakeEventPublicAsync(ev, staffUser);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(CptcEvents.Authorization.AuthorizationFailure.AlreadyPendingOrApproved, result.Failure);
    }

    [Fact]
    public async Task CanMakeEventPublic_AlreadyApproved_Fails()
    {
        // Arrange — UC9 A3: event is already in Approved status
        using var ctx = TestDbContextFactory.Create();
        var userManager = CreateMockUserManager();
        var service = CreateService(userManager, ctx);

        var ev = new Event
        {
            Title = "Approved Event",
            DateOfEvent = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            GroupId = 1,
            CreatedByUserId = "creator-1",
            IsAllDay = true,
            ApprovalStatus = ApprovalStatus.Approved
        };

        var staffUser = MakeUser("staff-1", "Staff");

        // Act
        var result = await service.CanMakeEventPublicAsync(ev, staffUser);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(CptcEvents.Authorization.AuthorizationFailure.AlreadyPendingOrApproved, result.Failure);
    }

    [Fact]
    public async Task CanMakeEventPublic_StudentCreatedEvent_Fails()
    {
        // Arrange — UC9 A2: event creator is a Student; public visibility not allowed regardless of requesting user's role
        using var ctx = TestDbContextFactory.Create();
        var userManager = CreateMockUserManager();
        var service = CreateService(userManager, ctx);

        // event created by a student user
        var studentCreator = new ApplicationUser { Id = "student-1", UserName = "student", FirstName = "Test", LastName = "Student" };
        userManager.Setup(u => u.FindByIdAsync("student-1")).ReturnsAsync(studentCreator);
        userManager.Setup(u => u.IsInRoleAsync(studentCreator, "Student")).ReturnsAsync(true);

        var ev = new Event
        {
            Title = "Student Event",
            DateOfEvent = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            GroupId = 1,
            CreatedByUserId = "student-1",
            IsAllDay = true
        };

        var staffUser = MakeUser("staff-1", "Staff");

        // Act - staff user should not be able to make the event public if the creator is a student
        var result = await service.CanMakeEventPublicAsync(ev, staffUser);

        // Assert
        Assert.False(result.Succeeded);
    }
}
