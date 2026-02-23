using CptcEvents.Data;
using CptcEvents.Models;
using Microsoft.EntityFrameworkCore;

namespace CptcEvents.Services;

/// <summary>
/// Service implementation for managing event RSVPs.
/// </summary>
public class RsvpService : IRsvpService
{
    private readonly ApplicationDbContext _context;

    public RsvpService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new RSVP for an event. If the user already has an RSVP, returns null.
    /// </summary>
    public async Task<EventRsvp?> CreateRsvpAsync(int eventId, string userId, RsvpStatus status)
    {
        // Check if event exists
        bool eventExists = await _context.Events.AnyAsync(e => e.Id == eventId);
        if (!eventExists)
        {
            return null;
        }

        // Check if user already has an RSVP for this event
        bool alreadyRsvped = await _context.EventRsvps
            .AnyAsync(r => r.EventId == eventId && r.UserId == userId);
        if (alreadyRsvped)
        {
            return null;
        }

        var rsvp = new EventRsvp
        {
            EventId = eventId,
            UserId = userId,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.EventRsvps.Add(rsvp);
        await _context.SaveChangesAsync();

        return rsvp;
    }

    /// <summary>
    /// Updates an existing RSVP status.
    /// </summary>
    public async Task<EventRsvp?> UpdateRsvpAsync(int rsvpId, RsvpStatus newStatus)
    {
        EventRsvp? rsvp = await _context.EventRsvps.FindAsync(rsvpId);
        if (rsvp == null)
        {
            return null;
        }

        rsvp.Status = newStatus;
        rsvp.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return rsvp;
    }

    /// <summary>
    /// Deletes an RSVP.
    /// </summary>
    public async Task<bool> DeleteRsvpAsync(int rsvpId)
    {
        EventRsvp? rsvp = await _context.EventRsvps.FindAsync(rsvpId);
        if (rsvp == null)
        {
            return false;
        }

        _context.EventRsvps.Remove(rsvp);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Gets an RSVP by its ID.
    /// </summary>
    public async Task<EventRsvp?> GetRsvpByIdAsync(int rsvpId)
    {
        return await _context.EventRsvps
            .Include(r => r.Event)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == rsvpId);
    }

    /// <summary>
    /// Gets a user's RSVP for a specific event.
    /// </summary>
    public async Task<EventRsvp?> GetUserRsvpForEventAsync(int eventId, string userId)
    {
        return await _context.EventRsvps
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);
    }

    /// <summary>
    /// Gets all RSVPs for a specific event.
    /// </summary>
    public async Task<List<EventRsvp>> GetRsvpsForEventAsync(int eventId)
    {
        return await _context.EventRsvps
            .Include(r => r.User)
            .Where(r => r.EventId == eventId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all RSVPs by a specific user.
    /// </summary>
    public async Task<List<EventRsvp>> GetRsvpsByUserAsync(string userId)
    {
        return await _context.EventRsvps
            .Include(r => r.Event)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets the count of RSVPs for an event grouped by status.
    /// </summary>
    public async Task<Dictionary<RsvpStatus, int>> GetRsvpCountsByStatusAsync(int eventId)
    {
        return await _context.EventRsvps
            .Where(r => r.EventId == eventId)
            .GroupBy(r => r.Status)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    /// <summary>
    /// Checks if a user has already RSVP'd to an event.
    /// </summary>
    public async Task<bool> HasUserRsvpedAsync(int eventId, string userId)
    {
        return await _context.EventRsvps
            .AnyAsync(r => r.EventId == eventId && r.UserId == userId);
    }

    /// <summary>
    /// Deletes all RSVPs for a specific event.
    /// </summary>
    public async Task<int> ClearAllRsvpsAsync(int eventId)
    {
        var rsvps = await _context.EventRsvps
            .Where(r => r.EventId == eventId)
            .ToListAsync();

        if (rsvps.Count == 0)
        {
            return 0;
        }

        _context.EventRsvps.RemoveRange(rsvps);
        await _context.SaveChangesAsync();
        return rsvps.Count;
    }
}
