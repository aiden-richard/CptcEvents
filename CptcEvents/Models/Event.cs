using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace CptcEvents.Models;

/// <summary>
/// This class represents an event in the website.
/// It is used to store information about events such as title, description, date, and location.
/// Some events are public and some are private.
/// </summary>
public class Event : IValidatableObject
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    public bool IsPublic { get; set; } = false;

    public bool IsAllDay { get; set; } = false;

    /// <summary>
    /// Day the event is taking place
    /// </summary>
    [Required]
    public DateOnly DateOfEvent { get; set; }

    /// <summary>
    /// Start time of the event
    /// </summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>
    /// End time of the event
    /// </summary>
    public TimeOnly EndTime { get; set; }

    [StringLength(2083)]
    [DataType(DataType.Url)]
    [Url]
    public string? Url { get; set; }

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
