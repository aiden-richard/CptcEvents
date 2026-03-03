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
    /// Deletes all RSVPs for a specific event using a single DELETE command.
    /// </summary>
    public async Task<int> ClearAllRsvpsAsync(int eventId)
    {
        return await _context.EventRsvps
            .Where(r => r.EventId == eventId)
            .ExecuteDeleteAsync();
    }

    /// <inheritdoc/>
    public async Task<Dictionary<int, EventRsvp>> GetUserRsvpsForEventsAsync(IEnumerable<int> eventIds, string userId)
    {
        var ids = eventIds.ToList();
        return await _context.EventRsvps
            .Where(r => ids.Contains(r.EventId) && r.UserId == userId)
            .ToDictionaryAsync(r => r.EventId);
    }

    /// <inheritdoc/>
    public async Task<Dictionary<int, Dictionary<RsvpStatus, int>>> GetRsvpCountsByStatusForEventsAsync(IEnumerable<int> eventIds)
    {
        var ids = eventIds.ToList();
        var grouped = await _context.EventRsvps
            .Where(r => ids.Contains(r.EventId))
            .GroupBy(r => new { r.EventId, r.Status })
            .Select(g => new { g.Key.EventId, g.Key.Status, Count = g.Count() })
            .ToListAsync();

        var result = new Dictionary<int, Dictionary<RsvpStatus, int>>();
        foreach (var item in grouped)
        {
            if (!result.ContainsKey(item.EventId))
            {
                result[item.EventId] = new Dictionary<RsvpStatus, int>();
            }
            result[item.EventId][item.Status] = item.Count;
        }
        return result;
    }
}
