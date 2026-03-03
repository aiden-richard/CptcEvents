using CptcEvents.Data;
using CptcEvents.Models;
using Microsoft.EntityFrameworkCore;

namespace CptcEvents.Services;

/// <summary>
/// Service interface for managing events in the application.
/// Provides CRUD operations and query methods for events, including filtering by group, user, and date range.
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Retrieves all events in the system.
    /// </summary>
    /// <returns>A collection of all events.</returns>
    Task<IEnumerable<Event>> GetAllEventsAsync();

    /// <summary>
    /// Retrieves all publicly visible events.
    /// </summary>
    /// <returns>A collection of events where <see cref="Event.IsPublic"/> is true.</returns>
    Task<IEnumerable<Event>> GetPublicEventsAsync();

    /// <summary>
    /// Retrieves all approved public events for display on the homepage.
    /// </summary>
    /// <returns>A collection of events where <see cref="Event.IsPublic"/> and <see cref="Event.IsApprovedPublic"/> are both true.</returns>
    Task<IEnumerable<Event>> GetApprovedPublicEventsAsync();

    /// <summary>
    /// Retrieves all events belonging to a specific group.
    /// </summary>
    /// <param name="groupId">The ID of the group to retrieve events for.</param>
    /// <returns>A collection of events ordered by date (descending) and start time.</returns>
    Task<IEnumerable<Event>> GetEventsForGroupAsync(int groupId);

    /// <summary>
    /// Retrieves all events visible to a specific user based on their group memberships.
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve events for.</param>
    /// <returns>A collection of events from groups where the user is a member, ordered by date (ascending) and start time.</returns>
    Task<IEnumerable<Event>> GetEventsForUserAsync(string userId);

    /// <summary>
    /// Retrieves all events upcoming visible to a specific user based on their group memberships.
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve events for.</param>
    /// <returns>A collection of upcoming events from groups where the user is a member, ordered by date (ascending) and start time.</returns>
    Task<IEnumerable<Event>> GetActiveEventsForUserAsync(string userId);

    /// <summary>
    /// Retrieves all events visible to a user, considering admin privileges.
    /// Admins can see all events regardless of group membership.
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve events for.</param>
    /// <param name="isAdmin">Whether the user has admin privileges.</param>
    /// <returns>A collection of events. All events for admins, or user's group events for non-admins.</returns>
    Task<IEnumerable<Event>> GetEventsForUserAsync(string userId, bool isAdmin);

    /// <summary>
    /// Retrieves all active (upcoming) events visible to a user, considering admin privileges.
    /// Admins can see all active events regardless of group membership.
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve events for.</param>
    /// <param name="isAdmin">Whether the user has admin privileges.</param>
    /// <returns>A collection of upcoming events. All upcoming events for admins, or user's group upcoming events for non-admins.</returns>
    Task<IEnumerable<Event>> GetActiveEventsForUserAsync(string userId, bool isAdmin);

    /// <summary>
    /// Retrieves a single event by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the event to retrieve.</param>
    /// <returns>The event if found; otherwise, null.</returns>
    Task<Event?> GetEventByIdAsync(int id);

    /// <summary>
    /// Creates a new event and persists it to the database.
    /// </summary>
    /// <param name="newEvent">The event entity to create.</param>
    /// <returns>The created event with its Group navigation property loaded.</returns>
    Task<Event> CreateEventAsync(Event newEvent);

    /// <summary>
    /// Updates an existing event with new values.
    /// </summary>
    /// <param name="eventId">The ID of the event to update.</param>
    /// <param name="updatedEvent">The event entity containing the updated values.</param>
    /// <returns>The updated event if found; otherwise, null.</returns>
    /// <remarks>The GroupId cannot be changed after creation.</remarks>
    Task<Event?> UpdateEventAsync(int eventId, Event updatedEvent);

    /// <summary>
    /// Deletes an event by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the event to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteEventAsync(int id);

    /// <summary>
    /// Retrieves all events within a specified date range.
    /// </summary>
    /// <param name="start">The start date of the range (inclusive).</param>
    /// <param name="end">The end date of the range (inclusive).</param>
    /// <returns>A collection of events occurring within the specified date range.</returns>
    Task<IEnumerable<Event>> GetEventsInRangeAsync(DateOnly start, DateOnly end);
}