namespace CptcEvents.Authorization.Requirements;

using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Authorization requirement that checks if a user is the owner of a specific group.
/// Used with <see cref="GroupOwnerHandler"/> to enforce group owner policies.
/// </summary>
public class GroupOwnerRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the route parameter key that contains the group ID.
    /// </summary>
    public string GroupIdRoute { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupOwnerRequirement"/> class.
    /// </summary>
    /// <param name="groupIdRoute">The route parameter key for the group ID (e.g., "groupId").</param>
    public GroupOwnerRequirement(string groupIdRoute)
    {
        GroupIdRoute = groupIdRoute;
    }
}
