using System.Security.Claims;
using CptcEvents.Authorization.Requirements;
using CptcEvents.Services;
using Microsoft.AspNetCore.Authorization;

namespace CptcEvents.Authorization.Handlers
{
    /// <summary>
    /// Authorization handler that checks if a user is a moderator (or owner) of a specific group.
    /// </summary>
    public class GroupModeratorHandler : AuthorizationHandler<GroupModeratorRequirement>
    {
        private readonly IGroupService _groupService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupModeratorHandler"/> class.
        /// </summary>
        /// <param name="groupService">The group service to check moderator status.</param>
        public GroupModeratorHandler(IGroupService groupService)
        {
            _groupService = groupService;
        }

        /// <inheritdoc/>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, GroupModeratorRequirement requirement)
        {
            // Get the user ID from the claims
            string? userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                context.Fail();
                return;
            }

            // Get httpContext
            if (context.Resource is not HttpContext httpContext)
            {
                context.Fail();
                return;
            }

            // Get groupId value as a string within an object
            httpContext.Request.RouteValues.TryGetValue(requirement.GroupIdRoute, out object? routeValue);
            if (routeValue == null)
            {
                context.Fail();
                return;
            }

            // Try to convert groupId string to int
            string groupIdStr = routeValue.ToString() ?? string.Empty;
            if (!int.TryParse(groupIdStr, out int groupId))
            {
                context.Fail();
                return;
            }

            // Check for moderator status (includes owners) once we have groupId and userId
            bool isModerator = await _groupService.IsUserModeratorAsync(groupId, userId);
            if (isModerator)
            {
                context.Succeed(requirement);
                return;
            }

            context.Fail();
        }
    }
}
