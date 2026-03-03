using System.Security.Claims;

namespace CptcEvents.Authorization.GroupAuthorizationService;

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
