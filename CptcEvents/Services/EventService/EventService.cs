using CptcEvents.Data;
using CptcEvents.Models;
using Microsoft.EntityFrameworkCore;

namespace CptcEvents.Services;



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
    public async Task<IEnumerable<Event>> GetApprovedPublicEventsAsync()
    {
        return await _context.Events
            .Include(e => e.Group)
            .Where(e => e.IsPublic && e.IsApprovedPublic)
            .OrderByDescending(e => e.DateOfEvent)
            .ThenBy(e => e.StartTime)
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
        existingEvent.IsRsvpEnabled = updatedEvent.IsRsvpEnabled;
        existingEvent.IsAllDay = updatedEvent.IsAllDay;
        existingEvent.DateOfEvent = updatedEvent.DateOfEvent;
        existingEvent.StartTime = updatedEvent.StartTime;
        existingEvent.EndTime = updatedEvent.EndTime;
        existingEvent.Url = updatedEvent.Url;
        existingEvent.BannerImageUrl = updatedEvent.BannerImageUrl;
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
            .Include(e => e.Group)
            .Where(e => e.DateOfEvent >= start && e.DateOfEvent <= end)
            .ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Event>> GetEventsForUserAsync(string userId, bool isAdmin)
    {
        if (string.IsNullOrEmpty(userId)) return Enumerable.Empty<Event>();

        if (isAdmin)
        {
            // Admins can see all events
            return await _context.Events
                .Include(e => e.Group)
                .OrderBy(e => e.DateOfEvent)
                .ThenBy(e => e.StartTime)
                .ToListAsync();
        }
        else
        {
            // Regular users see only events from groups they're members of
            return await GetEventsForUserAsync(userId);
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Event>> GetActiveEventsForUserAsync(string userId, bool isAdmin)
    {
        if (string.IsNullOrEmpty(userId)) return Enumerable.Empty<Event>();

        if (isAdmin)
        {
            // Admins can see all active events
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            return await _context.Events
                .Include(e => e.Group)
                .Where(e => e.DateOfEvent >= today)
                .OrderBy(e => e.DateOfEvent)
                .ThenBy(e => e.StartTime)
                .ToListAsync();
        }
        else
        {
            // Regular users see only active events from groups they're members of
            return await GetActiveEventsForUserAsync(userId);
        }
    }
}
