using CptcEvents.Data;
using CptcEvents.Models;
using Microsoft.EntityFrameworkCore;

namespace CptcEvents.Services;

public interface IEventService
{
    Task<IEnumerable<Event>> GetAllEventsAsync();
    Task<IEnumerable<Event>> GetPublicEventsAsync();
    Task<IEnumerable<Event>> GetEventsForGroupAsync(int groupId);
    Task<IEnumerable<Event>> GetEventsForUserAsync(string userId);
    Task<Event?> GetEventByIdAsync(int id);
    Task<Event> CreateEventAsync(Event newEvent);
    Task<Event?> UpdateEventAsync(int eventId, Event updatedEvent);
    Task DeleteEventAsync(int id);
    Task<IEnumerable<Event>> GetEventsInRangeAsync(DateOnly start, DateOnly end);
    
    // Legacy method - consider removing after migration
    Task AddEventAsync(Event newEvent);
}

public class EventService : IEventService
{
    private readonly ApplicationDbContext _context;
    public EventService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Event>> GetAllEventsAsync()
    {
        return await _context.Events
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetPublicEventsAsync()
    {
        return await _context.Events
            .Where(e => e.IsPublic)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetEventsForGroupAsync(int groupId)
    {
        return await _context.Events
            .Where(e => e.GroupId == groupId)
            .OrderByDescending(e => e.DateOfEvent)
            .ThenBy(e => e.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetEventsForUserAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return Enumerable.Empty<Event>();

        // Get events from groups where the user is a member
        return await _context.Events
            .Where(e => _context.GroupMemberships.Any(m => m.GroupId == e.GroupId && m.UserId == userId))
            .OrderByDescending(e => e.DateOfEvent)
            .ThenBy(e => e.StartTime)
            .ToListAsync();
    }

    public async Task<Event?> GetEventByIdAsync(int id)
    {
        return await _context.Events
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Event> CreateEventAsync(Event newEvent)
    {
        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();
        
        // Reload with navigation property
        return await _context.Events
            .Include(e => e.Group)
            .FirstAsync(e => e.Id == newEvent.Id);
    }

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

    public async Task DeleteEventAsync(int id)
    {
        Event? eventToDelete = await _context.Events.FindAsync(id);
        if (eventToDelete != null)
        {
            _context.Events.Remove(eventToDelete);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Event>> GetEventsInRangeAsync(DateOnly start, DateOnly end)
    {
        return await _context.Events
            .Where(e => e.DateOfEvent >= start && e.DateOfEvent <= end)
            .ToListAsync();
    }

    // Legacy method - kept for backward compatibility
    public async Task AddEventAsync(Event newEvent)
    {
        await CreateEventAsync(newEvent);
    }
}
