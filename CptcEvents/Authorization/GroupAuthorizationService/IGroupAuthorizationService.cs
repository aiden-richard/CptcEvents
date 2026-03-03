using System.Security.Claims;
using CptcEvents.Authorization;
using CptcEvents.Models;

namespace CptcEvents.Authorization.GroupAuthorizationService;


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
    Task<ServicesAuthorizationResult> EnsureMemberAsync(int groupId, ClaimsPrincipal user);

    /// <summary>
    /// Ensures the current user is a moderator (or owner) of the specified group.
    /// </summary>
    /// <param name="groupId">The ID of the group to check moderator status against.</param>
    /// <param name="user">The claims principal representing the current user.</param>
    /// <returns>A result indicating success or the specific authorization failure.</returns>
    Task<ServicesAuthorizationResult> EnsureModeratorAsync(int groupId, ClaimsPrincipal user);

    /// <summary>
    /// Ensures the current user is the owner of the specified group.
    /// </summary>
    /// <param name="groupId">The ID of the group to check ownership against.</param>
    /// <param name="user">The claims principal representing the current user.</param>
    /// <returns>A result indicating success or the specific authorization failure.</returns>
    Task<ServicesAuthorizationResult> EnsureOwnerAsync(int groupId, ClaimsPrincipal user);

    /// <summary>
    /// Determines whether the current user is an "effective owner" of the specified group —
    /// i.e. either the actual group owner or a site admin.
    /// Admins are treated as owners so they can perform owner-level UI actions.
    /// </summary>
    /// <param name="groupId">The ID of the group to check.</param>
    /// <param name="user">The claims principal representing the current user.</param>
    /// <returns>True if the user is the group owner or an admin; otherwise, false.</returns>
    Task<bool> IsEffectiveOwnerAsync(int groupId, ClaimsPrincipal user);

    /// <summary>
    /// Returns all groups visible to the user based on their role and group memberships.
    /// Admins see all groups; regular users see only groups they are members of.
    /// </summary>
    /// <param name="user">The claims principal representing the current user.</param>
    /// <returns>A collection of groups the user is permitted to see.</returns>
    Task<IEnumerable<Group>> GetVisibleGroupsForUserAsync(ClaimsPrincipal user);
}
