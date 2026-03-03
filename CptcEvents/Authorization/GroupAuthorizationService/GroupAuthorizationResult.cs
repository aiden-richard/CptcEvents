namespace CptcEvents.Authorization.GroupAuthorizationService;

using CptcEvents.Authorization;

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
/// Result of a group authorization check. Wrapper around <see cref="ServicesAuthorizationResult"/> 
/// with group-specific failure mappings for backward compatibility.
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

    /// <summary>
    /// Converts a <see cref="GroupAuthorizationResult"/> to a generic <see cref="ServicesAuthorizationResult"/>.
    /// </summary>
    public ServicesAuthorizationResult ToAuthorizationResult() => new(
        Succeeded,
        Failure switch
        {
            GroupAuthorizationFailure.None => AuthorizationFailure.None,
            GroupAuthorizationFailure.NotAuthenticated => AuthorizationFailure.NotAuthenticated,
            GroupAuthorizationFailure.GroupNotFound => AuthorizationFailure.ResourceNotFound,
            GroupAuthorizationFailure.NotMember => AuthorizationFailure.NotMember,
            GroupAuthorizationFailure.NotModerator => AuthorizationFailure.NotModerator,
            GroupAuthorizationFailure.NotOwner => AuthorizationFailure.NotOwner,
            _ => AuthorizationFailure.None
        }
    );
}