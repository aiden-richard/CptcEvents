using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CptcEvents.Models;

/// <summary>
/// View model for creating a new event.
/// </summary>
public class EventViewModel : IValidatableObject
{
    /// <summary>
    /// The title of the event.
    /// </summary>
    [Required]
    [StringLength(100)]
    public required string Title { get; set; }

    /// <summary>
    /// Optional description of the event.
    /// </summary>
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The group this event belongs to.
    /// </summary>
    [Required]
    [Display(Name = "Group")]
    public int GroupId { get; set; }

    /// <summary>
    /// Whether the event is publicly visible.
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// Whether the event is an all-day event.
    /// </summary>
    public bool IsAllDay { get; set; } = false;

    /// <summary>
    /// The date of the event.
    /// </summary>
    [Required]
    [Display(Name = "Date of Event")]
    public DateOnly DateOfEvent { get; set; }

    /// <summary>
    /// Start time of the event (required for non-all-day events).
    /// </summary>
    [Display(Name = "Start Time")]
    public TimeOnly StartTime { get; set; }

    /// <summary>
    /// End time of the event (required for non-all-day events).
    /// </summary>
    [Display(Name = "End Time")]
    public TimeOnly EndTime { get; set; }

    /// <summary>
    /// Optional URL for the event.
    /// </summary>
    [StringLength(2083)]
    [DataType(DataType.Url)]
    [Url]
    public string? Url { get; set; }

    /// <summary>
    /// Model-level validation. Ensures that for timed events the end time is after the start time.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IsAllDay && EndTime <= StartTime)
        {
            yield return new ValidationResult(
                "End time must be later than start time for timed events.",
                new[] { nameof(EndTime), nameof(StartTime) });
        }

        yield break;
    }
}
