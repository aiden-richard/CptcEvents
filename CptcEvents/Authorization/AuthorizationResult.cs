namespace CptcEvents.Authorization;

/// <summary>
/// Enumeration of possible authorization failures across different authorization contexts.
/// </summary>
public enum AuthorizationFailure
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
    /// The requested resource (group, event, etc.) was not found.
    /// </summary>
    ResourceNotFound,

    /// <summary>
    /// User is not a member of the resource.
    /// </summary>
    NotMember,

    /// <summary>
    /// User is not a moderator (or higher) for the resource.
    /// </summary>
    NotModerator,

    /// <summary>
    /// User is not the owner of the resource.
    /// </summary>
    NotOwner
}

/// <summary>
/// Result of an authorization check containing success status and failure reason (if applicable).
/// Used by both event and group authorization services to provide a consistent result pattern.
/// </summary>
public record AuthorizationResult(bool Succeeded, AuthorizationFailure Failure)
{
    /// <summary>
    /// Creates a successful authorization result.
    /// </summary>
    /// <returns>A result indicating authorization succeeded.</returns>
    public static AuthorizationResult Success() => new(true, AuthorizationFailure.None);

    /// <summary>
    /// Creates a failed authorization result with the specified failure reason.
    /// </summary>
    /// <param name="failure">The reason authorization failed.</param>
    /// <returns>A result indicating authorization failed with the specified reason.</returns>
    public static AuthorizationResult Fail(AuthorizationFailure failure) => new(false, failure);
}
