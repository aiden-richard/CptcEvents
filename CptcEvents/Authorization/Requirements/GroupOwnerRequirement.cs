namespace CptcEvents.Authorization.Requirements
{
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Authorization requirement that checks if a user is the owner of a specific group.
    /// </summary>
    public class GroupOwnerRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// The route key for the group ID.
        /// </summary>
        public string GroupIdRoute { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupOwnerRequirement"/> class.
        /// </summary>
        /// <param name="groupIdRoute">The route key for the group ID.</param>
        public GroupOwnerRequirement(string groupIdRoute)
        {
            GroupIdRoute = groupIdRoute;
        }
    }
}
