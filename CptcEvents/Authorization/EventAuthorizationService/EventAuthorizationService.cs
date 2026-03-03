using System.Security.Claims;
using CptcEvents.Services;
using Microsoft.AspNetCore.Identity;
using CptcEvents.Models;
using CptcEvents.Authorization.GroupAuthorizationService;

namespace CptcEvents.Authorization.EventAuthorizationService;

/// <inheritdoc/>
public class EventAuthorizationService : IEventAuthorizationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IGroupService _groupService;
    private readonly IEventService _eventService;

    public EventAuthorizationService(
        UserManager<ApplicationUser> userManager,
        IGroupService groupService,
        IEventService eventService)
    {
        _userManager = userManager;
        _groupService = groupService;
        _eventService = eventService;
    }

    #region Event Access Control

    /// <inheritdoc/>
    public async Task<GroupAuthorizationResult> CanViewEventAsync(Event eventItem, ClaimsPrincipal user)
    {
        // Public approved events are visible to everyone including anonymous users
        if (eventItem.IsPublic && eventItem.IsApprovedPublic)
        {
            return GroupAuthorizationResult.Success();
        }

        string? userId = user.Identity?.IsAuthenticated == true ? _userManager.GetUserId(user) : null;
        if (userId == null)
        {
            return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.NotAuthenticated);
        }

        // Admins can view any event
        if (user.IsInRole("Admin"))
        {
            return GroupAuthorizationResult.Success();
        }

        // Private events require group membership
        bool isMember = await _groupService.IsUserMemberAsync(eventItem.GroupId, userId);
        if (!isMember)
        {
            return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.NotMember);
        }

        return GroupAuthorizationResult.Success();
    }

    /// <inheritdoc/>
    public async Task<GroupAuthorizationResult> CanEditEventAsync(Event eventItem, ClaimsPrincipal user)
    {
        string? userId = user.Identity?.IsAuthenticated == true ? _userManager.GetUserId(user) : null;
        if (userId == null)
        {
            return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.NotAuthenticated);
        }

        // Admins can edit any event
        if (user.IsInRole("Admin"))
        {
            return GroupAuthorizationResult.Success();
        }

        // User must be a moderator in the event's group
        bool isModerator = await _groupService.IsUserModeratorAsync(eventItem.GroupId, userId);
        if (!isModerator)
        {
            return GroupAuthorizationResult.Fail(GroupAuthorizationFailure.NotModerator);
        }

        return GroupAuthorizationResult.Success();
    }

    #endregion

    #region Event Retrieval

    /// <inheritdoc/>
    public async Task<IEnumerable<Event>> GetVisibleEventsForUserAsync(ClaimsPrincipal user)
    {
        string? userId = user.Identity?.IsAuthenticated == true ? _userManager.GetUserId(user) : null;
        if (userId == null) return Enumerable.Empty<Event>();

        if (user.IsInRole("Admin"))
        {
            return await _eventService.GetAllEventsAsync();
        }

        return await _eventService.GetEventsForUserAsync(userId);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Event>> GetActiveVisibleEventsForUserAsync(ClaimsPrincipal user)
    {
        string? userId = user.Identity?.IsAuthenticated == true ? _userManager.GetUserId(user) : null;
        if (userId == null) return Enumerable.Empty<Event>();

        if (user.IsInRole("Admin"))
        {
            return await _eventService.GetActiveEventsForUserAsync(userId, isAdmin: true);
        }

        return await _eventService.GetActiveEventsForUserAsync(userId, isAdmin: false);
    }

    #endregion
}