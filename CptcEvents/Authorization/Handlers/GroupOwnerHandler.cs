using System.Security.Claims;
using CptcEvents.Authorization.Requirements;
using CptcEvents.Services;
using Microsoft.AspNetCore.Authorization;

namespace CptcEvents.Authorization.Handlers;

/// <summary>
/// Authorization handler that enforces <see cref="GroupOwnerRequirement"/>.
/// Checks if a user is the owner of a specific group extracted from the route data.
/// </summary>
public class GroupOwnerHandler : AuthorizationHandler<GroupOwnerRequirement>
{
    private readonly IGroupService _groupService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupOwnerHandler"/> class.
    /// </summary>
    /// <param name="groupService">The group service to check owner status.</param>
    public GroupOwnerHandler(IGroupService groupService)
    {
        _groupService = groupService;
    }

    /// <inheritdoc/>
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, GroupOwnerRequirement requirement)
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

        // Admins have owner privileges in all groups
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }

        // Check for owner status once we have groupId and userId
        bool isOwner = await _groupService.IsUserOwnerAsync(groupId, userId);
        if (isOwner)
        {
            context.Succeed(requirement);
            return;
        }

        context.Fail();
    }
}
