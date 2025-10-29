using CptcEventHub.Data;
using CptcEventHub.Models;
using Microsoft.EntityFrameworkCore;

namespace CptcEvents.Services;

public interface IEventService
{
    Task<IEnumerable<Event>> GetAllEventsAsync();
    Task<Event?> GetEventByIdAsync(int id);
    Task AddEventAsync(Event newEvent);
    Task UpdateEventAsync(Event updatedEvent);
    Task DeleteEventAsync(int id);
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
        return await _context.Events.ToListAsync();
    }
    public async Task<Event?> GetEventByIdAsync(int id)
    {
        return await _context.Events.FindAsync(id);
    }
    public async Task AddEventAsync(Event newEvent)
    {
        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateEventAsync(Event updatedEvent)
    {
        _context.Events.Update(updatedEvent);
        await _context.SaveChangesAsync();
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
}
