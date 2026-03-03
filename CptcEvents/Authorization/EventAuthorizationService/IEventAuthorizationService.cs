using System.Security.Claims;
using CptcEvents.Models;
using CptcEvents.Authorization;

namespace CptcEvents.Authorization.EventAuthorizationService;

/// <summary>
/// Contract for authorizing user actions against event resources.
/// </summary>
public interface IEventAuthorizationService
{
    /// <summary>
    /// Determines whether the current user can view the given event.
    /// Public approved events are visible to everyone; private events require group membership.
    /// Admins can view any event.
    /// </summary>
    /// <param name="eventItem">The event to check access against.</param>
    /// <param name="user">The claims principal representing the current user.</param>
    /// <returns>A result indicating success or the specific authorization failure.</returns>
    Task<ServicesAuthorizationResult> CanViewEventAsync(Event eventItem, ClaimsPrincipal user);

    /// <summary>
    /// Determines whether the current user can edit the given event.
    /// Requires moderator status in the event's group.
    /// Admins can edit any event.
    /// </summary>
    /// <param name="eventItem">The event to check edit access against.</param>
    /// <param name="user">The claims principal representing the current user.</param>
    /// <returns>A result indicating success or the specific authorization failure.</returns>
    Task<ServicesAuthorizationResult> CanEditEventAsync(Event eventItem, ClaimsPrincipal user);

    /// <summary>
    /// Returns all events visible to the user based on their role and group memberships.
    /// Admins see all events; regular users see only events from their groups.
    /// </summary>
    /// <param name="user">The claims principal representing the current user.</param>
    /// <returns>A collection of events the user is permitted to see.</returns>
    Task<IEnumerable<Event>> GetVisibleEventsForUserAsync(ClaimsPrincipal user);

    /// <summary>
    /// Returns all upcoming (active) events visible to the user based on their role and group memberships.
    /// Admins see all active events; regular users see only upcoming events from their groups.
    /// </summary>
    /// <param name="user">The claims principal representing the current user.</param>
    /// <returns>A collection of upcoming events the user is permitted to see.</returns>
    Task<IEnumerable<Event>> GetActiveVisibleEventsForUserAsync(ClaimsPrincipal user);
}