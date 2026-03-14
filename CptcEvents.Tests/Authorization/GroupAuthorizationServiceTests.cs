using System.Security.Claims;
using CptcEvents.Authorization.GroupAuthorizationService;
using CptcEvents.Models;
using CptcEvents.Services;
using CptcEvents.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace CptcEvents.Tests.Authorization;

/// <summary>
/// Unit tests for <see cref="GroupAuthorizationService"/>.
/// Covers UC7 A1 (Create an event — user lacks Moderator or Owner role).
/// </summary>
public class GroupAuthorizationServiceTests
{
    private static Mock<UserManager<ApplicationUser>> CreateMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        // Mirror the real UserManager.GetUserId behaviour: extract the NameIdentifier claim
        mock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns<ClaimsPrincipal>(p => p.FindFirstValue(ClaimTypes.NameIdentifier));

        return mock;
    }

    private static ClaimsPrincipal MakeUser(string userId)
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    // UC7 A1: user does not hold Moderator or Owner role

    [Fact]
    public async Task EnsureModerator_MemberOnly_Fails()
    {
        // Arrange — user-2 is a plain Member, not Moderator or Owner
        using var ctx = TestDbContextFactory.Create();
        var groupService = new GroupService(ctx);
        var userManager = CreateMockUserManager();
        var service = new GroupAuthorizationService(groupService, userManager.Object);

        var group = new Group { Name = "Group", OwnerId = "owner-1", PrivacyLevel = PrivacyLevel.Public };
        await groupService.CreateGroupAsync(group);
        await groupService.AddUserToGroupAsync(group.Id, "user-2", RoleType.Member);

        var memberUser = MakeUser("user-2");

        // Act
        var result = await service.EnsureModeratorAsync(group.Id, memberUser);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(CptcEvents.Authorization.AuthorizationFailure.NotModerator, result.Failure);
    }

    [Fact]
    public async Task EnsureModerator_Moderator_Succeeds()
    {
        // Arrange — user-2 holds Moderator role (UC7 main flow)
        using var ctx = TestDbContextFactory.Create();
        var groupService = new GroupService(ctx);
        var userManager = CreateMockUserManager();
        var service = new GroupAuthorizationService(groupService, userManager.Object);

        var group = new Group { Name = "Group", OwnerId = "owner-1", PrivacyLevel = PrivacyLevel.Public };
        await groupService.CreateGroupAsync(group);
        await groupService.AddUserToGroupAsync(group.Id, "user-2", RoleType.Moderator);

        var moderatorUser = MakeUser("user-2");

        // Act
        var result = await service.EnsureModeratorAsync(group.Id, moderatorUser);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task EnsureModerator_Owner_Succeeds()
    {
        // Arrange — owner is implicitly a Moderator (UC7 main flow)
        using var ctx = TestDbContextFactory.Create();
        var groupService = new GroupService(ctx);
        var userManager = CreateMockUserManager();
        var service = new GroupAuthorizationService(groupService, userManager.Object);

        var group = new Group { Name = "Group", OwnerId = "owner-1", PrivacyLevel = PrivacyLevel.Public };
        await groupService.CreateGroupAsync(group);

        var ownerUser = MakeUser("owner-1");

        // Act
        var result = await service.EnsureModeratorAsync(group.Id, ownerUser);

        // Assert
        Assert.True(result.Succeeded);
    }

    // UC6 A1: user does not hold Owner role

    [Fact]
    public async Task EnsureOwner_MemberOnly_Fails()
    {
        // Arrange — UC6 A1: user holds Member role, not Owner
        using var ctx = TestDbContextFactory.Create();
        var groupService = new GroupService(ctx);
        var userManager = CreateMockUserManager();
        var service = new GroupAuthorizationService(groupService, userManager.Object);

        var group = new Group { Name = "Group", OwnerId = "owner-1", PrivacyLevel = PrivacyLevel.Public };
        await groupService.CreateGroupAsync(group);
        await groupService.AddUserToGroupAsync(group.Id, "user-2", RoleType.Member);

        var memberUser = MakeUser("user-2");

        // Act
        var result = await service.EnsureOwnerAsync(group.Id, memberUser);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(CptcEvents.Authorization.AuthorizationFailure.NotOwner, result.Failure);
    }

    [Fact]
    public async Task EnsureOwner_Owner_Succeeds()
    {
        // Arrange — UC6 main flow: user holds Owner role
        using var ctx = TestDbContextFactory.Create();
        var groupService = new GroupService(ctx);
        var userManager = CreateMockUserManager();
        var service = new GroupAuthorizationService(groupService, userManager.Object);

        var group = new Group { Name = "Group", OwnerId = "owner-1", PrivacyLevel = PrivacyLevel.Public };
        await groupService.CreateGroupAsync(group);

        var ownerUser = MakeUser("owner-1");

        // Act
        var result = await service.EnsureOwnerAsync(group.Id, ownerUser);

        // Assert
        Assert.True(result.Succeeded);
    }
}
