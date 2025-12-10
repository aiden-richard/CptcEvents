namespace CptcEvents.Authorization.Requirements;

using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Authorization requirement that checks if a user is a moderator (or owner) of a specific group.
/// Used with <see cref="GroupModeratorHandler"/> to enforce group moderator policies.
/// </summary>
public class GroupModeratorRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Gets the route parameter key that contains the group ID.
    /// </summary>
    public string GroupIdRoute { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupModeratorRequirement"/> class.
    /// </summary>
    /// <param name="groupIdRoute">The route parameter key for the group ID (e.g., "groupId").</param>
    public GroupModeratorRequirement(string groupIdRoute)
    {
        GroupIdRoute = groupIdRoute;
    }
}
