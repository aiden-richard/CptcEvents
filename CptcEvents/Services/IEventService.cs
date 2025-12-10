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

/// <summary>
/// Implementation of <see cref="IEventService"/> that provides event management functionality
/// using Entity Framework Core and the application's database context.
/// </summary>
public class EventService : IEventService
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventService"/> class.
    /// </summary>
    /// <param name="context">The application database context for data access.</param>
    public EventService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Event>> GetAllEventsAsync()
    {
        return await _context.Events
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Event>> GetPublicEventsAsync()
    {
        return await _context.Events
            .Where(e => e.IsPublic)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Event>> GetEventsForGroupAsync(int groupId)
    {
        return await _context.Events
            .Where(e => e.GroupId == groupId)
            .OrderByDescending(e => e.DateOfEvent)
            .ThenBy(e => e.StartTime)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Event>> GetEventsForUserAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return Enumerable.Empty<Event>();

        // Get events from groups where the user is a member
        return await _context.Events
            .Include(e => e.Group)
            .Where(e => _context.GroupMemberships.Any(m => m.GroupId == e.GroupId && m.UserId == userId))
            .OrderBy(e => e.DateOfEvent)
            .ThenBy(e => e.StartTime)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Event>> GetActiveEventsForUserAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return Enumerable.Empty<Event>();

        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Get upcoming events from groups where the user is a member
        return await _context.Events
            .Include(e => e.Group)
            .Where(e => e.DateOfEvent >= today &&
                        _context.GroupMemberships.Any(m => m.GroupId == e.GroupId && m.UserId == userId))
            .OrderBy(e => e.DateOfEvent)
            .ThenBy(e => e.StartTime)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<Event?> GetEventByIdAsync(int id)
    {
        return await _context.Events
            .Include(e => e.Group)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    /// <inheritdoc/>
    public async Task<Event> CreateEventAsync(Event newEvent)
    {
        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();
        
        // Reload with navigation property
        return await _context.Events
            .Include(e => e.Group)
            .FirstAsync(e => e.Id == newEvent.Id);
    }

    /// <inheritdoc/>
    public async Task<Event?> UpdateEventAsync(int eventId, Event updatedEvent)
    {
        Event? existingEvent = await _context.Events.FindAsync(eventId);
        if (existingEvent == null)
        {
            return null;
        }

        // Update event properties
        existingEvent.Title = updatedEvent.Title;
        existingEvent.Description = updatedEvent.Description;
        existingEvent.IsPublic = updatedEvent.IsPublic;
        existingEvent.IsAllDay = updatedEvent.IsAllDay;
        existingEvent.DateOfEvent = updatedEvent.DateOfEvent;
        existingEvent.StartTime = updatedEvent.StartTime;
        existingEvent.EndTime = updatedEvent.EndTime;
        existingEvent.Url = updatedEvent.Url;
        // Note: GroupId should not be changed after creation

        await _context.SaveChangesAsync();
        return existingEvent;
    }

    /// <inheritdoc/>
    public async Task DeleteEventAsync(int id)
    {
        Event? eventToDelete = await _context.Events.FindAsync(id);
        if (eventToDelete != null)
        {
            _context.Events.Remove(eventToDelete);
            await _context.SaveChangesAsync();
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Event>> GetEventsInRangeAsync(DateOnly start, DateOnly end)
    {
        return await _context.Events
            .Where(e => e.DateOfEvent >= start && e.DateOfEvent <= end)
            .ToListAsync();
    }
}
