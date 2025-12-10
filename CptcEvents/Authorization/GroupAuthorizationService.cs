using System.Security.Claims;
using CptcEvents.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CptcEvents.Models;

namespace CptcEvents.Authorization;

/// <summary>
/// Enumeration of possible authorization failures when checking group membership or roles.
/// </summary>
public enum GroupAuthorizationFailure
{
    /// <summary>
    /// No failure; authorization succeeded.
    /// </summary>
    None,

    /// <summary>
    /// User is not authenticated.
    /// </summary>
    NotAuthenticated,

    /// <summary>
    /// The requested group was not found.
    /// </summary>
    GroupNotFound,

    /// <summary>
    /// User is not a member of the group.
    /// </summary>
    NotMember,

    /// <summary>
    /// User is not a moderator (or higher) in the group.
    /// </summary>
    NotModerator,

    /// <summary>
    /// User is not the owner of the group.
    /// </summary>
    NotOwner
}

/// <summary>
/// Result of a group authorization check containing success status and failure reason (if applicable).
/// </summary>
public record GroupAuthorizationResult(bool Succeeded, GroupAuthorizationFailure Failure)
{
    /// <summary>
    /// Creates a successful authorization result.
    /// </summary>
    /// <returns>A result indicating authorization succeeded.</returns>
    public static GroupAuthorizationResult Success() => new(true, GroupAuthorizationFailure.None);

    /// <summary>
    /// Creates a failed authorization result with the specified failure reason.
    /// </summary>
    /// <param name="failure">The reason authorization failed.</param>
    /// <returns>A result indicating authorization failed with the specified reason.</returns>
    public static GroupAuthorizationResult Fail(GroupAuthorizationFailure failure) => new(false, failure);
}

/// <summary>
/// Contract for authorizing user actions against group membership and role requirements.
/// </summary>
public interface IGroupAuthorizationService
{
    /// <summary>
    /// Extracts the user ID from the claims principal if the user is authenticated.
    /// </summary>
    /// <param name="user">The claims principal representing the current user.</param>
    /// <returns>The user ID if authenticated; otherwise, null.</returns>
    Task<string?> GetUserIdAsync(ClaimsPrincipal user);

    /// <summary>
    /// Ensures the current user is a member of the specified group.
    /// </summary>
    /// <param name="groupId">The ID of the group to check membership against.</param>
    /// <param name="user">The claims principal representing the current user.</param>
    /// <returns>A result indicating success or the specific authorization failure.</returns>
    Task<GroupAuthorizationResult> EnsureMemberAsync(int groupId, ClaimsPrincipal user);

    /// <summary>
    /// Ensures the current user is a moderator (or owner) of the specified group.
    /// </summary>
    /// <param name="groupId">The ID of the group to check moderator status against.</param>
    /// <param name="user">The claims principal representing the current user.</param>
    /// <returns>A result indicating success or the specific authorization failure.</returns>
    Task<GroupAuthorizationResult> EnsureModeratorAsync(int groupId, ClaimsPrincipal user);

    /// <summary>
    /// Ensures the current user is the owner of the specified group.
    /// </summary>
    /// <param name="groupId">The ID of the group to check ownership against.</param>
    /// <param name="user">The claims principal representing the current user.</param>
    /// <returns>A result indicating success or the specific authorization failure.</returns>
    Task<GroupAuthorizationResult> EnsureOwnerAsync(int groupId, ClaimsPrincipal user);
}

/// <summary>
/// Implementation of <see cref="IGroupAuthorizationService"/> that centralizes membership and role checks.
/// Provides consistent authorization logic to keep controllers thin and reduce duplication.
/// </summary>
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
            GroupAuthorizationFailure.NotMember => controller.Forbid(),
            GroupAuthorizationFailure.NotModerator => controller.Forbid(),
            GroupAuthorizationFailure.NotOwner => controller.Forbid(),
            _ => controller.Forbid()
        };
    }
}
