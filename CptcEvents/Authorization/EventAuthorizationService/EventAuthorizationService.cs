using System.Security.Claims;
using CptcEvents.Services;
using Microsoft.AspNetCore.Identity;
using CptcEvents.Models;
using CptcEvents.Authorization;

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
    public async Task<ServicesAuthorizationResult> CanViewEventAsync(Event eventItem, ClaimsPrincipal user)
    {
        // Public approved events are visible to everyone including anonymous users
        if (eventItem.IsPublic && eventItem.IsApprovedPublic)
        {
            return ServicesAuthorizationResult.Success();
        }

        string? userId = user.Identity?.IsAuthenticated == true ? _userManager.GetUserId(user) : null;
        if (userId == null)
        {
            return ServicesAuthorizationResult.Fail(AuthorizationFailure.NotAuthenticated);
        }

        // Admins can view any event
        if (user.IsInRole("Admin"))
        {
            return ServicesAuthorizationResult.Success();
        }

        // Private events require group membership
        bool isMember = await _groupService.IsUserMemberAsync(eventItem.GroupId, userId);
        if (!isMember)
        {
            return ServicesAuthorizationResult.Fail(AuthorizationFailure.NotMember);
        }

        return ServicesAuthorizationResult.Success();
    }

    /// <inheritdoc/>
    public async Task<ServicesAuthorizationResult> CanEditEventAsync(Event eventItem, ClaimsPrincipal user)
    {
        string? userId = user.Identity?.IsAuthenticated == true ? _userManager.GetUserId(user) : null;
        if (userId == null)
        {
            return ServicesAuthorizationResult.Fail(AuthorizationFailure.NotAuthenticated);
        }

        // Admins can edit any event
        if (user.IsInRole("Admin"))
        {
            return ServicesAuthorizationResult.Success();
        }

        // User must be a moderator in the event's group
        bool isModerator = await _groupService.IsUserModeratorAsync(eventItem.GroupId, userId);
        if (!isModerator)
        {
            return ServicesAuthorizationResult.Fail(AuthorizationFailure.NotModerator);
        }

        return ServicesAuthorizationResult.Success();
    }

    #endregion

    /// <inheritdoc/>
    public async Task<ServicesAuthorizationResult> CanMakeEventPublicAsync(Event? existingEvent, ClaimsPrincipal user)
    {
        // If editing an existing event whose creator is a Student, deny regardless of the requesting user's role
        if (existingEvent != null)
        {
            var creator = await _userManager.FindByIdAsync(existingEvent.CreatedByUserId);
            if (creator != null && await _userManager.IsInRoleAsync(creator, "Student"))
            {
                return ServicesAuthorizationResult.Fail(AuthorizationFailure.CreatorIsStudent);
            }
        }

        // Only Staff or Admin may make events public
        if (!user.IsInRole("Staff") && !user.IsInRole("Admin"))
        {
            return ServicesAuthorizationResult.Fail(AuthorizationFailure.NotStaff);
        }

        return ServicesAuthorizationResult.Success();
    }

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