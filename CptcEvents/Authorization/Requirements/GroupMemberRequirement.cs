namespace CptcEvents.Authorization.Requirements
{
    using Microsoft.AspNetCore.Authorization;

    /// <summary>
    /// Authorization requirement that checks if a user is a member of a specific group.
    /// </summary>
    public class GroupMemberRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// The ID of the group to check membership against.
        /// </summary>
        public string GroupIdRoute { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMemberRequirement"/> class.
        /// </summary>
        /// <param name="groupIdRoute">The route key for the group ID.</param>
        public GroupMemberRequirement(string groupIdRoute)
        {
            GroupIdRoute = groupIdRoute;
        }
    }
}