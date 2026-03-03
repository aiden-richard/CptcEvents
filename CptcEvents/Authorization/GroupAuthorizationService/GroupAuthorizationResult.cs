namespace CptcEvents.Authorization.GroupAuthorizationService;

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