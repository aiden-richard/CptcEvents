using CptcEvents.Models;

namespace CptcEvents.Services;

/// <summary>
/// Service interface for managing event RSVPs.
/// Provides operations for creating, updating, deleting, and querying RSVP responses.
/// </summary>
public interface IRsvpService
{
    /// <summary>
    /// Creates a new RSVP for an event.
    /// </summary>
    /// <param name="eventId">The ID of the event to RSVP to.</param>
    /// <param name="userId">The ID of the user creating the RSVP.</param>
    /// <param name="status">The RSVP status (Going, Maybe, NotGoing).</param>
    /// <returns>The created EventRsvp, or null if creation failed.</returns>
    Task<EventRsvp?> CreateRsvpAsync(int eventId, string userId, RsvpStatus status);

    /// <summary>
    /// Updates an existing RSVP status.
    /// </summary>
    /// <param name="rsvpId">The ID of the RSVP to update.</param>
    /// <param name="newStatus">The new RSVP status.</param>
    /// <returns>The updated EventRsvp, or null if update failed.</returns>
    Task<EventRsvp?> UpdateRsvpAsync(int rsvpId, RsvpStatus newStatus);

    /// <summary>
    /// Deletes an RSVP.
    /// </summary>
    /// <param name="rsvpId">The ID of the RSVP to delete.</param>
    /// <returns>True if deletion was successful, false otherwise.</returns>
    Task<bool> DeleteRsvpAsync(int rsvpId);

    /// <summary>
    /// Gets an RSVP by its ID.
    /// </summary>
    /// <param name="rsvpId">The ID of the RSVP.</param>
    /// <returns>The EventRsvp if found, null otherwise.</returns>
    Task<EventRsvp?> GetRsvpByIdAsync(int rsvpId);

    /// <summary>
    /// Gets a user's RSVP for a specific event.
    /// </summary>
    /// <param name="eventId">The ID of the event.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>The EventRsvp if found, null otherwise.</returns>
    Task<EventRsvp?> GetUserRsvpForEventAsync(int eventId, string userId);

    /// <summary>
    /// Gets all RSVPs for a specific event.
    /// </summary>
    /// <param name="eventId">The ID of the event.</param>
    /// <returns>A list of all RSVPs for the event.</returns>
    Task<List<EventRsvp>> GetRsvpsForEventAsync(int eventId);

    /// <summary>
    /// Gets all RSVPs by a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A list of all RSVPs created by the user.</returns>
    Task<List<EventRsvp>> GetRsvpsByUserAsync(string userId);

    /// <summary>
    /// Gets the count of RSVPs for an event grouped by status.
    /// </summary>
    /// <param name="eventId">The ID of the event.</param>
    /// <returns>A dictionary with RSVP status as key and count as value.</returns>
    Task<Dictionary<RsvpStatus, int>> GetRsvpCountsByStatusAsync(int eventId);

    /// <summary>
    /// Checks if a user has already RSVP'd to an event.
    /// </summary>
    /// <param name="eventId">The ID of the event.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>True if the user has an RSVP for the event, false otherwise.</returns>
    Task<bool> HasUserRsvpedAsync(int eventId, string userId);
}
