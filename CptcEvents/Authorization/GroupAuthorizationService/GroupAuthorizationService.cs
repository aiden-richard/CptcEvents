using System.Security.Claims;
using CptcEvents.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CptcEvents.Models;

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
    public async Task<GroupAuthorizationResult> EnsureMemberAsync(int groupId, ClaimsPrincipal user)
    {
        string? userId = await GetUserIdAsync(user);
        if (userId == null)
        {
            return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.NotAuthenticated);
        }

        // Admins can access any group
        if (user.IsInRole("Admin"))
        {
            Group? adminGroup = await _groupService.GetGroupByIdAsync(groupId);
            if (adminGroup == null)
            {
                return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.GroupNotFound);
            }
            return GroupAuthorizationResult.Success();
        }

        Group? group = await _groupService.GetGroupByIdAsync(groupId);
        if (group == null)
        {
            return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.GroupNotFound);
        }

        bool isMember = await _groupService.IsUserMemberAsync(groupId, userId);
        if (!isMember)
        {
            return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.NotMember);
        }

        return GroupAuthorizationResult.Success();
    }

    /// <inheritdoc/>
    public async Task<GroupAuthorizationResult> EnsureModeratorAsync(int groupId, ClaimsPrincipal user)
    {
        string? userId = await GetUserIdAsync(user);
        if (userId == null)
        {
            return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.NotAuthenticated);
        }

        // Admins have moderator privileges in any group
        if (user.IsInRole("Admin"))
        {
            Group? adminGroup = await _groupService.GetGroupByIdAsync(groupId);
            if (adminGroup == null)
            {
                return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.GroupNotFound);
            }
            return GroupAuthorizationResult.Success();
        }

        Group? group = await _groupService.GetGroupByIdAsync(groupId);
        if (group == null)
        {
            return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.GroupNotFound);
        }

        bool isModerator = await _groupService.IsUserModeratorAsync(groupId, userId);
        if (!isModerator)
        {
            return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.NotModerator);
        }

        return GroupAuthorizationResult.Success();
    }

    /// <inheritdoc/>
    public async Task<GroupAuthorizationResult> EnsureOwnerAsync(int groupId, ClaimsPrincipal user)
    {
        string? userId = await GetUserIdAsync(user);
        if (userId == null)
        {
            return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.NotAuthenticated);
        }

        // Admins have owner privileges in any group
        if (user.IsInRole("Admin"))
        {
            Group? adminGroup = await _groupService.GetGroupByIdAsync(groupId);
            if (adminGroup == null)
            {
                return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.GroupNotFound);
            }
            return GroupAuthorizationResult.Success();
        }

        Group? group = await _groupService.GetGroupByIdAsync(groupId);
        if (group == null)
        {
            return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.GroupNotFound);
        }

        bool isOwner = await _groupService.IsUserOwnerAsync(groupId, userId);
        if (!isOwner)
        {
            return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.NotOwner);
        }

        return GroupAuthorizationResult.Success();
    }
}

/// <summary>
/// Extension methods for <see cref="GroupAuthorizationResult"/> to convert authorization failures into appropriate HTTP responses.
/// </summary>
public static class GroupAuthorizationResultExtensions
{
    /// <summary>
    /// Converts a failed authorization result into an appropriate HTTP action result.
    /// </summary>
    /// <param name="result">The authorization result to convert.</param>
    /// <param name="controller">The controller context for creating action results.</param>
    /// <returns>An action result appropriate for the failure reason (Challenge, NotFound, or Forbid).</returns>
    public static IActionResult ToActionResult(this GroupAuthorizationResult result, Controller controller)
    {
        return result.Failure switch
        {
            GroupAuthorizationFailure.NotAuthenticated => controller.Challenge(),
            GroupAuthorizationFailure.GroupNotFound => controller.NotFound(),
            GroupAuthorizationFailure.NotMember => controller.RedirectToAction("Index"),
            GroupAuthorizationFailure.NotModerator => controller.Forbid(),
            GroupAuthorizationFailure.NotOwner => controller.Forbid(),
            _ => controller.Forbid()
        };
    }
}
