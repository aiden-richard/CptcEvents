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
    [StringLength(10000)]
    public string? Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the approval status of this event for public display.
    /// </summary>
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Private;

    /// <summary>
    /// Gets or sets a value indicating whether RSVP is enabled for this event.
    /// </summary>
    public bool IsRsvpEnabled { get; set; } = false;

    /// <summary>
    /// Optional cutoff date/time after which RSVPs are no longer accepted.
    /// If null, RSVPs are accepted up until the event date.
    /// </summary>
    public DateTime? RsvpCutoffAt { get; set; }

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
    /// Optional URL associated with the event
    /// </summary>
    [StringLength(100)]
    [DataType(DataType.Url)]
    [Url]
    public string? Url { get; set; }

    /// <summary>
    /// Optional URL for a banner image stored in Azure Blob Storage.
    /// </summary>
    [StringLength(500)]
    public string? BannerImageUrl { get; set; }

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

/// <summary>
/// Defines the approval workflow states for public event visibility.
/// </summary>
public enum ApprovalStatus
{
    [Display(Name = "Private")]
    Private,

    [Display(Name = "Pending Approval")]
    PendingApproval,

    [Display(Name = "Approved")]
    Approved,

    [Display(Name = "Denied")]
    Denied
}