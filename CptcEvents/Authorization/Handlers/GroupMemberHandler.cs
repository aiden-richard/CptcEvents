using System.Security.Claims;
using System.Text.RegularExpressions;
using CptcEvents.Authorization.Requirements;
using CptcEvents.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace CptcEvents.Authorization.Handlers
{
    /// <summary>
    /// Authorization handler that checks if a user is a member of a specific group.
    /// </summary>
    public class GroupMemberHandler : AuthorizationHandler<GroupMemberRequirement>
    {
        private readonly IGroupService _groupService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMemberHandler"/> class.
        /// </summary>
        /// <param name="groupService">The group service to check membership.</param>
        public GroupMemberHandler(IGroupService groupService)
        {
            _groupService = groupService;
        }

        /// <inheritdoc/>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, GroupMemberRequirement requirement)
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
            object? routeValue = null;
            httpContext.Request.RouteValues.TryGetValue(requirement.GroupIdRoute, out routeValue);
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
            
            // Check for membership record once we have groupId and userId
            bool isMember = await _groupService.IsUserMemberAsync(groupId, userId);
            if (isMember)
            {
                context.Succeed(requirement);
                return;
            }


            
            
        }
    }
}