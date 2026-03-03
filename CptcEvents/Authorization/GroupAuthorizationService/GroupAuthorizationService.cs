using System.Security.Claims;
using CptcEvents.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CptcEvents.Models;
using CptcEvents.Authorization;

namespace CptcEvents.Authorization.GroupAuthorizationService;

/// <inheritdoc/>
public class GroupAuthorizationService : IGroupAuthorizationService
{
    private readonly IGroupService _groupService;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupAuthorizationService"/> class.
    /// </summary>
    /// <param name="groupService">The group service for membership and role queries.</param>
    /// <param name="userManager">The user manager for identity operations.</param>
    public GroupAuthorizationService(IGroupService groupService, UserManager<ApplicationUser> userManager)
    {
        _groupService = groupService;
        _userManager = userManager;
    }

    /// <inheritdoc/>
    public Task<string?> GetUserIdAsync(ClaimsPrincipal user)
    {
        return Task.FromResult(user.Identity?.IsAuthenticated == true ? _userManager.GetUserId(user) : null);
    }

    /// <inheritdoc/>
    public async Task<ServicesAuthorizationResult> EnsureMemberAsync(int groupId, ClaimsPrincipal user)
    {
        string? userId = await GetUserIdAsync(user);
        if (userId == null)
        {
            return ServicesAuthorizationResult.Fail(AuthorizationFailure.NotAuthenticated);
        }

        // Admins can access any group
        if (user.IsInRole("Admin"))
        {
            Group? adminGroup = await _groupService.GetGroupByIdAsync(groupId);
            if (adminGroup == null)
            {
                return ServicesAuthorizationResult.Fail(AuthorizationFailure.ResourceNotFound);
            }
            return ServicesAuthorizationResult.Success();
        }

        Group? group = await _groupService.GetGroupByIdAsync(groupId);
        if (group == null)
        {
            return ServicesAuthorizationResult.Fail(AuthorizationFailure.ResourceNotFound);
        }

        bool isMember = await _groupService.IsUserMemberAsync(groupId, userId);
        if (!isMember)
        {
            return ServicesAuthorizationResult.Fail(AuthorizationFailure.NotMember);
        }

        return ServicesAuthorizationResult.Success();
    }

    /// <inheritdoc/>
    public async Task<ServicesAuthorizationResult> EnsureModeratorAsync(int groupId, ClaimsPrincipal user)
    {
        string? userId = await GetUserIdAsync(user);
        if (userId == null)
        {
            return ServicesAuthorizationResult.Fail(AuthorizationFailure.NotAuthenticated);
        }

        // Admins have moderator privileges in any group
        if (user.IsInRole("Admin"))
        {
            Group? adminGroup = await _groupService.GetGroupByIdAsync(groupId);
            if (adminGroup == null)
            {
                return ServicesAuthorizationResult.Fail(AuthorizationFailure.ResourceNotFound);
            }
            return ServicesAuthorizationResult.Success();
        }

        Group? group = await _groupService.GetGroupByIdAsync(groupId);
        if (group == null)
        {
            return ServicesAuthorizationResult.Fail(AuthorizationFailure.ResourceNotFound);
        }

        bool isModerator = await _groupService.IsUserModeratorAsync(groupId, userId);
        if (!isModerator)
        {
            return ServicesAuthorizationResult.Fail(AuthorizationFailure.NotModerator);
        }

        return ServicesAuthorizationResult.Success();
    }

    /// <inheritdoc/>
    public async Task<ServicesAuthorizationResult> EnsureOwnerAsync(int groupId, ClaimsPrincipal user)
    {
        string? userId = await GetUserIdAsync(user);
        if (userId == null)
        {
            return ServicesAuthorizationResult.Fail(AuthorizationFailure.NotAuthenticated);
        }

        // Admins have owner privileges in any group
        if (user.IsInRole("Admin"))
        {
            Group? adminGroup = await _groupService.GetGroupByIdAsync(groupId);
            if (adminGroup == null)
            {
                return ServicesAuthorizationResult.Fail(AuthorizationFailure.ResourceNotFound);
            }
            return ServicesAuthorizationResult.Success();
        }

        Group? group = await _groupService.GetGroupByIdAsync(groupId);
        if (group == null)
        {
            return ServicesAuthorizationResult.Fail(AuthorizationFailure.ResourceNotFound);
        }

        bool isOwner = await _groupService.IsUserOwnerAsync(groupId, userId);
        if (!isOwner)
        {
            return ServicesAuthorizationResult.Fail(AuthorizationFailure.NotOwner);
        }

        return ServicesAuthorizationResult.Success();
    }

    /// <inheritdoc/>
    public async Task<bool> IsEffectiveOwnerAsync(int groupId, ClaimsPrincipal user)
    {
        string? userId = await GetUserIdAsync(user);
        if (userId == null) return false;

        if (user.IsInRole("Admin")) return true;

        return await _groupService.IsUserOwnerAsync(groupId, userId);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Group>> GetVisibleGroupsForUserAsync(ClaimsPrincipal user)
    {
        string? userId = await GetUserIdAsync(user);
        if (userId == null) return Enumerable.Empty<Group>();

        bool isAdmin = user.IsInRole("Admin");
        return await _groupService.GetGroupsForUserAsync(userId, isAdmin);
    }
}

/// <summary>
/// Backward compatibility extension for <see cref="GroupAuthorizationResult"/>.
/// Deprecated: Use <see cref="AuthorizationResultExtensions.ToActionResult"/> instead.
/// </summary>
public static class GroupAuthorizationResultExtensions
{
    /// <summary>
    /// Converts a <see cref="GroupAuthorizationResult"/> to an action result.
    /// Deprecated: Use <see cref="AuthorizationResultExtensions.ToActionResult"/> instead.
    /// </summary>
    public static IActionResult ToActionResult(this GroupAuthorizationResult result, Controller controller)
    {
        return result.ToAuthorizationResult().ToActionResult(controller);
    }
}
