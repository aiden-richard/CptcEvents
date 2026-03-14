using CptcEvents.Models;
using CptcEvents.Services;
using CptcEvents.Tests.Helpers;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace CptcEvents.Tests.Services;

/// <summary>
/// Unit tests for <see cref="InviteService"/>.
/// Covers UC4 (Invite a user to a group) and UC5 (Redeem a group invite).
/// </summary>
public class InviteServiceTests
{
    private static Mock<UserManager<ApplicationUser>> CreateMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static async Task<(Data.ApplicationDbContext ctx, GroupService groupService, Group group)>
        SetupGroupAsync(string ownerId = "owner-1")
    {
        var ctx = TestDbContextFactory.Create();
        var groupService = new GroupService(ctx);
        var group = new Group
        {
            Name = "Test Group",
            OwnerId = ownerId,
            PrivacyLevel = PrivacyLevel.Public,
            InvitePolicy = GroupInvitePolicy.OwnerOnly
        };
        await groupService.CreateGroupAsync(group);
        return (ctx, groupService, group);
    }

    // UC4: Invite a user to a group

    [Fact]
    public async Task ValidateCreateInvite_OwnerOnly_OwnerSucceeds()
    {
        // Arrange
        var (ctx, groupService, group) = await SetupGroupAsync("owner-1");
        var userManager = CreateMockUserManager();
        var service = new InviteService(ctx, groupService, userManager.Object);

        var viewModel = new GroupInviteViewModel
        {
            GroupId = group.Id,
            OneTimeUse = true,
            Expires = false
        };

        // Act
        var result = await service.ValidateCreateInviteAsync("owner-1", viewModel);

        // Assert
        Assert.True(result.IsValid);
        Assert.False(result.Unauthorized);
    }

    [Fact]
    public async Task ValidateCreateInvite_OwnerOnly_NonOwnerFails()
    {
        // Arrange — UC4 A1: user does not meet role criteria (OwnerOnly policy, non-owner)
        var (ctx, groupService, group) = await SetupGroupAsync("owner-1");
        var userManager = CreateMockUserManager();
        var service = new InviteService(ctx, groupService, userManager.Object);

        var viewModel = new GroupInviteViewModel
        {
            GroupId = group.Id,
            OneTimeUse = true,
            Expires = false
        };

        // Act — "other-user" is not the owner
        var result = await service.ValidateCreateInviteAsync("other-user", viewModel);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Unauthorized);
    }

    [Fact]
    public async Task ValidateCreateInvite_EmptyCurrentUser_FailsUnauthorized()
    {
        // Arrange
        var (ctx, groupService, group) = await SetupGroupAsync("owner-1");
        var userManager = CreateMockUserManager();
        var service = new InviteService(ctx, groupService, userManager.Object);

        var viewModel = new GroupInviteViewModel
        {
            GroupId = group.Id,
            OneTimeUse = true,
            Expires = false
        };

        // Act
        var result = await service.ValidateCreateInviteAsync("", viewModel);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Unauthorized);
    }

    [Fact]
    public async Task ValidateCreateInvite_GroupNotFound_FailsNotFound()
    {
        // Arrange
        var ctx = TestDbContextFactory.Create();
        var groupService = new GroupService(ctx);
        var userManager = CreateMockUserManager();
        var service = new InviteService(ctx, groupService, userManager.Object);

        var viewModel = new GroupInviteViewModel
        {
            GroupId = 9999,
            OneTimeUse = true,
            Expires = false
        };

        // Act
        var result = await service.ValidateCreateInviteAsync("owner-1", viewModel);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.NotFound);
    }

    [Fact]
    public async Task ValidateCreateInvite_TargetUsernameNotFound_Fails()
    {
        // Arrange
        var (ctx, groupService, group) = await SetupGroupAsync("owner-1");
        var userManager = CreateMockUserManager();
        userManager.Setup(m => m.FindByNameAsync("missing-user")).ReturnsAsync((ApplicationUser?)null);
        var service = new InviteService(ctx, groupService, userManager.Object);

        var viewModel = new GroupInviteViewModel
        {
            GroupId = group.Id,
            Username = "missing-user",
            OneTimeUse = true,
            Expires = false
        };

        // Act
        var result = await service.ValidateCreateInviteAsync("owner-1", viewModel);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.FieldErrors.ContainsKey("Username"));
    }

    [Fact]
    public async Task ValidateCreateInvite_SelfInvite_Fails()
    {
        // Arrange
        var (ctx, groupService, group) = await SetupGroupAsync("owner-1");
        var userManager = CreateMockUserManager();
        userManager.Setup(m => m.FindByNameAsync("owner-1")).ReturnsAsync(new ApplicationUser
        {
            Id = "owner-1",
            UserName = "owner-1",
            FirstName = "Owner",
            LastName = "User"
        });
        var service = new InviteService(ctx, groupService, userManager.Object);

        var viewModel = new GroupInviteViewModel
        {
            GroupId = group.Id,
            Username = "owner-1",
            OneTimeUse = true,
            Expires = false
        };

        // Act
        var result = await service.ValidateCreateInviteAsync("owner-1", viewModel);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.FieldErrors.ContainsKey("Username"));
    }

    [Fact]
    public async Task ValidateCreateInvite_TargetedInviteMultiUse_Fails()
    {
        // Arrange
        var (ctx, groupService, group) = await SetupGroupAsync("owner-1");
        var userManager = CreateMockUserManager();
        userManager.Setup(m => m.FindByNameAsync("student-1")).ReturnsAsync(new ApplicationUser
        {
            Id = "student-1",
            UserName = "student-1",
            FirstName = "Student",
            LastName = "One"
        });
        var service = new InviteService(ctx, groupService, userManager.Object);

        var viewModel = new GroupInviteViewModel
        {
            GroupId = group.Id,
            Username = "student-1",
            OneTimeUse = false,
            Expires = false
        };

        // Act
        var result = await service.ValidateCreateInviteAsync("owner-1", viewModel);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.FieldErrors.ContainsKey("OneTimeUse"));
    }

    [Fact]
    public async Task ValidateCreateInvite_ExpiresWithoutExpiresAt_Fails()
    {
        // Arrange
        var (ctx, groupService, group) = await SetupGroupAsync("owner-1");
        var userManager = CreateMockUserManager();
        var service = new InviteService(ctx, groupService, userManager.Object);

        var viewModel = new GroupInviteViewModel
        {
            GroupId = group.Id,
            OneTimeUse = true,
            Expires = true,
            ExpiresAt = null
        };

        // Act
        var result = await service.ValidateCreateInviteAsync("owner-1", viewModel);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.FieldErrors.ContainsKey("ExpiresAt"));
    }

    [Fact]
    public async Task ValidateCreateInvite_ExpiresInPast_Fails()
    {
        // Arrange
        var (ctx, groupService, group) = await SetupGroupAsync("owner-1");
        var userManager = CreateMockUserManager();
        var service = new InviteService(ctx, groupService, userManager.Object);

        var viewModel = new GroupInviteViewModel
        {
            GroupId = group.Id,
            OneTimeUse = true,
            Expires = true,
            ExpiresAt = DateTime.Now.AddMinutes(-5)
        };

        // Act
        var result = await service.ValidateCreateInviteAsync("owner-1", viewModel);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.FieldErrors.ContainsKey("ExpiresAt"));
    }

    [Fact]
    public async Task ValidateUpdateInvite_TargetedInviteMultiUse_Fails()
    {
        // Arrange
        var (ctx, groupService, group) = await SetupGroupAsync("owner-1");
        var userManager = CreateMockUserManager();
        var service = new InviteService(ctx, groupService, userManager.Object);

        var invite = new GroupInvite
        {
            GroupId = group.Id,
            InviteCode = "TARGETUPD",
            CreatedById = "owner-1",
            InvitedUserId = "student-1",
            OneTimeUse = true
        };

        var editModel = new GroupInviteEditViewModel
        {
            Id = invite.Id,
            GroupId = group.Id,
            Expires = false,
            OneTimeUse = false
        };

        // Act
        var result = await service.ValidateUpdateInviteAsync("owner-1", invite, editModel);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.FieldErrors.ContainsKey("OneTimeUse"));
    }

    // UC5: Redeem a group invite

    [Fact]
    public async Task RedeemInvite_ValidInvite_AddsMember()
    {
        // Arrange
        var (ctx, groupService, group) = await SetupGroupAsync("owner-1");
        var userManager = CreateMockUserManager();
        var service = new InviteService(ctx, groupService, userManager.Object);

        var memberCountBefore = group.MemberCount;

        var invite = new GroupInvite
        {
            GroupId = group.Id,
            InviteCode = "TESTCODE",
            CreatedById = "owner-1",
            OneTimeUse = false
        };
        ctx.GroupInvites.Add(invite);
        await ctx.SaveChangesAsync();

        // Act
        var member = await service.RedeemInviteAsync(invite.Id, "user-2");

        // Assert
        Assert.NotNull(member);
        Assert.Equal("user-2", member.UserId);
        Assert.Equal(group.Id, member.GroupId);
        Assert.Equal(memberCountBefore + 1, group.MemberCount);
    }

    [Fact]
    public async Task RedeemInvite_NotFound_ReturnsNull()
    {
        // Arrange
        var (ctx, groupService, _) = await SetupGroupAsync("owner-1");
        var userManager = CreateMockUserManager();
        var service = new InviteService(ctx, groupService, userManager.Object);

        // Act
        var member = await service.RedeemInviteAsync(9999, "user-2");

        // Assert
        Assert.Null(member);
    }

    [Fact]
    public async Task RedeemInvite_ValidInvite_IncrementsTimesUsed()
    {
        // Arrange
        var (ctx, groupService, group) = await SetupGroupAsync("owner-1");
        var userManager = CreateMockUserManager();
        var service = new InviteService(ctx, groupService, userManager.Object);

        var invite = new GroupInvite
        {
            GroupId = group.Id,
            InviteCode = "COUNTCODE",
            CreatedById = "owner-1",
            OneTimeUse = false,
            TimesUsed = 0
        };
        ctx.GroupInvites.Add(invite);
        await ctx.SaveChangesAsync();

        // Act
        _ = await service.RedeemInviteAsync(invite.Id, "user-2");

        // Assert
        var refreshed = await ctx.GroupInvites.FindAsync(invite.Id);
        Assert.NotNull(refreshed);
        Assert.Equal(1, refreshed!.TimesUsed);
    }

    [Fact]
    public async Task RedeemInvite_ExpiredByTimesUsed_ReturnsNull()
    {
        // Arrange — UC5 A1: invite code has already been used (OneTimeUse + TimesUsed >= 1)
        var (ctx, groupService, group) = await SetupGroupAsync("owner-1");
        var userManager = CreateMockUserManager();
        var service = new InviteService(ctx, groupService, userManager.Object);

        var invite = new GroupInvite
        {
            GroupId = group.Id,
            InviteCode = "USEDCODE",
            CreatedById = "owner-1",
            OneTimeUse = true,
            TimesUsed = 1  // already used once → IsExpired = true
        };
        ctx.GroupInvites.Add(invite);
        await ctx.SaveChangesAsync();

        // Act
        var member = await service.RedeemInviteAsync(invite.Id, "user-2");

        // Assert
        Assert.Null(member);
    }

    [Fact]
    public async Task RedeemInvite_ExpiredByExpiresAt_ReturnsNull()
    {
        // Arrange — UC5 A1: invite is expired based on ExpiresAt
        var (ctx, groupService, group) = await SetupGroupAsync("owner-1");
        var userManager = CreateMockUserManager();
        var service = new InviteService(ctx, groupService, userManager.Object);

        var invite = new GroupInvite
        {
            GroupId = group.Id,
            InviteCode = "EXPIREDCODE",
            CreatedById = "owner-1",
            OneTimeUse = false,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        };
        ctx.GroupInvites.Add(invite);
        await ctx.SaveChangesAsync();

        // Act
        var member = await service.RedeemInviteAsync(invite.Id, "user-2");

        // Assert
        Assert.Null(member);
    }

    [Fact]
    public async Task RedeemInvite_AlreadyMember_ReturnsNull()
    {
        // Arrange — UC5 A2: user is already a member of the group
        var (ctx, groupService, group) = await SetupGroupAsync("owner-1");
        var userManager = CreateMockUserManager();
        var service = new InviteService(ctx, groupService, userManager.Object);

        // Pre-add user-2 as member
        await groupService.AddUserToGroupAsync(group.Id, "user-2", RoleType.Member);

        var invite = new GroupInvite
        {
            GroupId = group.Id,
            InviteCode = "FRESHCODE",
            CreatedById = "owner-1",
            OneTimeUse = false
        };
        ctx.GroupInvites.Add(invite);
        await ctx.SaveChangesAsync();

        // Act
        var member = await service.RedeemInviteAsync(invite.Id, "user-2");

        // Assert
        Assert.Null(member);
    }
}
