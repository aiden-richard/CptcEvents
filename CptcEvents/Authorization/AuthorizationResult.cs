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
    NotOwner,

    /// <summary>
    /// User does not have a staff-level role (Staff or Admin) required for the action.
    /// </summary>
    NotStaff,

    /// <summary>
    /// The event was created by a Student and therefore cannot be made public.
    /// </summary>
    CreatorIsStudent,

    /// <summary>
    /// The invite is expired or has already been fully used.
    /// </summary>
    InviteExpired,

    /// <summary>
    /// The invite is restricted to a specific user who is not the current user.
    /// </summary>
    InviteNotForUser,

    /// <summary>
    /// The user is already a member of the group and cannot redeem this invite.
    /// </summary>
    AlreadyMember,

    /// <summary>
    /// The event is already Pending or Approved for public display.
    /// </summary>
    AlreadyPendingOrApproved
}

/// <summary>
/// Result of an authorization check containing success status and failure reason (if applicable).
/// Used by both event and group authorization services to provide a consistent result pattern.
/// </summary>
public record ServicesAuthorizationResult(bool Succeeded, AuthorizationFailure Failure)
{
    /// <summary>
    /// Creates a successful authorization result.
    /// </summary>
    /// <returns>A result indicating authorization succeeded.</returns>
    public static ServicesAuthorizationResult Success() => new(true, AuthorizationFailure.None);

    /// <summary>
    /// Creates a failed authorization result with the specified failure reason.
    /// </summary>
    /// <param name="failure">The reason authorization failed.</param>
    /// <returns>A result indicating authorization failed with the specified reason.</returns>
    public static ServicesAuthorizationResult Fail(AuthorizationFailure failure) => new(false, failure);
}
