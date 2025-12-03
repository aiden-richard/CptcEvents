namespace CptcEvents.Authorization.Requirements
{
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Authorization requirement that checks if a user is a moderator (or owner) of a specific group.
    /// </summary>
    public class GroupModeratorRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// The route key for the group ID.
        /// </summary>
        public string GroupIdRoute { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupModeratorRequirement"/> class.
        /// </summary>
        /// <param name="groupIdRoute">The route key for the group ID.</param>
        public GroupModeratorRequirement(string groupIdRoute)
        {
            GroupIdRoute = groupIdRoute;
        }
    }
}
