using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace CptcEvents.Models;

/// <summary>
/// This class represents an event in the website.
/// It is used to store information about events such as title, description, date, and location.
/// Some events are public and some are private.
/// </summary>
public class Event : IValidatableObject
{
    /// <summary>
    /// Primary key for the event record.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// The title of the event.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional description providing details about the event.
    /// </summary>
    [StringLength(1000)]
    public string? Description { get; set; } = null;

    /// <summary>
    /// Gets or sets a value indicating whether this event is marked as public.
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether this public event has been approved for display on the homepage.
    /// </summary>
    public bool IsApprovedPublic { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether this public event has been denied and should not be displayed.
    /// </summary>
    public bool IsDeniedPublic { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether this event spans the entire day (has no specific start/end times).
    /// </summary>
    public bool IsAllDay { get; set; } = false;

    /// <summary>
    /// Day the event is taking place.
    /// </summary>
    [Required]
    public DateOnly DateOfEvent { get; set; }

    /// <summary>
    /// Start time of the event (only applicable for timed events where IsAllDay is false).
    /// </summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>
    /// End time of the event (only applicable for timed events where IsAllDay is false).
    /// </summary>
    public TimeOnly EndTime { get; set; }

    /// <summary>
    /// Optional URL associated with the event (e.g., event details page, registration link).
    /// </summary>
    [StringLength(100)]
    [DataType(DataType.Url)]
    [Url]
    public string? Url { get; set; }

    /// <summary>
    /// Foreign key to the <see cref="Group"/> this event belongs to.
    /// </summary>
    [Display(Name = "Group")]
    public int GroupId { get; set; }

    /// <summary>
    /// Navigation property for the group this event belongs to.
    /// </summary>
    [ForeignKey("GroupId")]
    public Group? Group { get; set; }

    /// <summary>
    /// Foreign key to the <see cref="ApplicationUser"/> who created this event.
    /// </summary>
    public string CreatedByUserId { get; set; } = null!;

    /// <summary>
    /// Navigation property for the user who created the event.
    /// </summary>
    [ForeignKey("CreatedByUserId")]
    public ApplicationUser? CreatedByUser { get; set; }

    /// <summary>
    /// Model-level validation. Ensures that for timed events the end time is after the start time.
    /// Additional cross-property rules can be added here.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IsAllDay)
        {
            // For timed events ensure the end is after the start within the same date
            if (EndTime <= StartTime)
            {
                yield return new ValidationResult("End time must be later than start time for timed events.", new[] { nameof(EndTime), nameof(StartTime) });
            }
        }

        yield break;
    }
}
