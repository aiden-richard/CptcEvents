using CptcEvents.Models;
using CptcEvents.Services;
using CptcEvents.Tests.Helpers;

namespace CptcEvents.Tests.Services;

/// <summary>
/// Unit tests for <see cref="GroupService"/>.
/// Covers UC2 (Create a group), UC3 (Join a group), and UC6 (Manage group members).
/// </summary>
public class GroupServiceTests
{
    // UC2: Create a group

    [Fact]
    public async Task CreateGroup_PersistsGroup()
    {
        // Arrange
        using var ctx = TestDbContextFactory.Create();
        var service = new GroupService(ctx);

        var group = new Group
        {
            Name = "Test Group",
            OwnerId = "user-1",
            PrivacyLevel = PrivacyLevel.Public
        };

        // Act
        var created = await service.CreateGroupAsync(group);

        // Assert
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal("Test Group", created.Name);
        Assert.Equal("user-1", created.OwnerId);
    }

    [Fact]
    public async Task CreateGroup_AssignsOwnerRoleMembership()
    {
        // Arrange
        using var ctx = TestDbContextFactory.Create();
        var service = new GroupService(ctx);

        var group = new Group
        {
            Name = "Test Group",
            OwnerId = "user-1",
            PrivacyLevel = PrivacyLevel.Public
        };

        // Act
        var created = await service.CreateGroupAsync(group);

        var membership = ctx.GroupMemberships
            .FirstOrDefault(m => m.GroupId == created.Id && m.UserId == "user-1");

        // Assert
        Assert.NotNull(membership);
        Assert.Equal(RoleType.Owner, membership.Role);
    }

    // UC3: Join a group

    [Fact]
    public async Task AddUserToGroup_AddsUserAsMember()
    {
        // Arrange
        using var ctx = TestDbContextFactory.Create();
        var service = new GroupService(ctx);

        var group = new Group { Name = "Public Group", OwnerId = "owner-1", PrivacyLevel = PrivacyLevel.Public };
        await service.CreateGroupAsync(group);

        // Act
        var member = await service.AddUserToGroupAsync(group.Id, "user-2", RoleType.Member);

        // Assert
        Assert.NotNull(member);
        Assert.Equal("user-2", member.UserId);
        Assert.Equal(RoleType.Member, member.Role);
    }

    [Fact]
    public async Task AddUserToGroup_AlreadyMember_ReturnsNull()
    {
        // Arrange — UC3 A2: user is already a member
        using var ctx = TestDbContextFactory.Create();
        var service = new GroupService(ctx);

        var group = new Group { Name = "Group", OwnerId = "owner-1", PrivacyLevel = PrivacyLevel.Public };
        await service.CreateGroupAsync(group);
        await service.AddUserToGroupAsync(group.Id, "user-2", RoleType.Member);

        // Act
        var duplicate = await service.AddUserToGroupAsync(group.Id, "user-2", RoleType.Member);

        // Assert
        Assert.Null(duplicate);
    }

    [Fact]
    public async Task AddUserToGroup_PrivateGroup_ReturnsNull()
    {
        // Arrange — UC3 A1: group privacy level is Private; direct join not permitted
        using var ctx = TestDbContextFactory.Create();
        var service = new GroupService(ctx);

        var group = new Group { Name = "Private Group", OwnerId = "owner-1", PrivacyLevel = PrivacyLevel.RequiresInvite };
        await service.CreateGroupAsync(group);

        // Act
        var member = await service.AddUserToGroupAsync(group.Id, "user-2", RoleType.Member);

        // Assert
        Assert.Null(member);
    }

    [Fact]
    public async Task AddUserToGroup_GroupNotFound_ReturnsNull()
    {
        // Arrange
        using var ctx = TestDbContextFactory.Create();
        var service = new GroupService(ctx);

        // Act
        var member = await service.AddUserToGroupAsync(9999, "user-2", RoleType.Member);

        // Assert
        Assert.Null(member);
    }

    // UC6: Manage group members

    [Fact]
    public async Task UpdateUserRole_ChangesMemberRole()
    {
        // Arrange
        using var ctx = TestDbContextFactory.Create();
        var service = new GroupService(ctx);

        var group = new Group { Name = "Group", OwnerId = "owner-1", PrivacyLevel = PrivacyLevel.Public };
        await service.CreateGroupAsync(group);
        await service.AddUserToGroupAsync(group.Id, "user-2", RoleType.Member);

        // Act
        var updated = await service.UpdateUserRoleAsync(group.Id, "user-2", RoleType.Moderator);

        // Assert
        Assert.NotNull(updated);
        Assert.Equal(RoleType.Moderator, updated.Role);
    }

    [Fact]
    public async Task UpdateUserRole_CannotDemoteOwner_ReturnsNull()
    {
        // Arrange — UC6 A3: owner cannot have their own role changed
        using var ctx = TestDbContextFactory.Create();
        var service = new GroupService(ctx);

        var group = new Group { Name = "Group", OwnerId = "owner-1", PrivacyLevel = PrivacyLevel.Public };
        await service.CreateGroupAsync(group);

        // Act — attempt to change the owner's role
        var result = await service.UpdateUserRoleAsync(group.Id, "owner-1", RoleType.Member);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateUserRole_NonMember_ReturnsNull()
    {
        // Arrange — UC6 A2: target user is not a member
        using var ctx = TestDbContextFactory.Create();
        var service = new GroupService(ctx);

        var group = new Group { Name = "Group", OwnerId = "owner-1", PrivacyLevel = PrivacyLevel.Public };
        await service.CreateGroupAsync(group);

        // Act
        var result = await service.UpdateUserRoleAsync(group.Id, "user-2", RoleType.Moderator);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task RemoveUserFromGroup_RemovesMember()
    {
        // Arrange
        using var ctx = TestDbContextFactory.Create();
        var service = new GroupService(ctx);

        var group = new Group { Name = "Group", OwnerId = "owner-1", PrivacyLevel = PrivacyLevel.Public };
        await service.CreateGroupAsync(group);
        await service.AddUserToGroupAsync(group.Id, "user-2", RoleType.Member);

        // Act
        await service.RemoveUserFromGroupAsync(group.Id, "user-2");

        // Assert
        bool stillMember = await service.IsUserMemberAsync(group.Id, "user-2");
        Assert.False(stillMember);
    }

    [Fact]
    public async Task RemoveUserFromGroup_NonMember_DoesNothing()
    {
        // Arrange — UC6 A2: target user is not a member
        using var ctx = TestDbContextFactory.Create();
        var service = new GroupService(ctx);

        var group = new Group { Name = "Group", OwnerId = "owner-1", PrivacyLevel = PrivacyLevel.Public };
        await service.CreateGroupAsync(group);

        // Act — should not throw
        var exception = await Record.ExceptionAsync(
            () => service.RemoveUserFromGroupAsync(group.Id, "not-a-member"));

        // Assert
        Assert.Null(exception);
    }
}
