using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CptcEvents.Models;

/// <summary>
/// Represents an RSVP from a user for a specific event.
/// </summary>
public class EventRsvp
{
    /// <summary>
    /// Primary key for the RSVP record.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the <see cref="Event"/> this RSVP is for.
    /// </summary>
    [Required]
    public int EventId { get; set; }

    /// <summary>
    /// Navigation property for the event this RSVP belongs to.
    /// </summary>
    [ForeignKey("EventId")]
    public Event? Event { get; set; }

    /// <summary>
    /// Foreign key to the <see cref="ApplicationUser"/> who created this RSVP.
    /// </summary>
    [Required]
    public string UserId { get; set; } = null!;

    /// <summary>
    /// Navigation property for the user who created the RSVP.
    /// </summary>
    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// The RSVP status indicating the user's response to the event.
    /// </summary>
    [Required]
    public RsvpStatus Status { get; set; } = RsvpStatus.Going;

    /// <summary>
    /// Timestamp when the RSVP was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the RSVP was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Enumeration of possible RSVP statuses.
/// </summary>
public enum RsvpStatus
{
    /// <summary>
    /// User plans to attend the event.
    /// </summary>
    Going,

    /// <summary>
    /// User might attend the event.
    /// </summary>
    Maybe,

    /// <summary>
    /// User does not plan to attend the event.
    /// </summary>
    NotGoing
}
