namespace CptcEvents.Models.ViewModels;

/// <summary>
/// View model for the ApprovePublicEvents admin page.
/// Contains pending, approved, and denied public events.
/// </summary>
public class ApprovePublicEventsViewModel
{
    /// <summary>
    /// Public events that are pending approval (IsPublic = true, IsApprovedPublic = false, IsDeniedPublic = false).
    /// </summary>
    public IEnumerable<Event> PendingEvents { get; set; } = new List<Event>();

    /// <summary>
    /// Public events that have been approved (IsPublic = true, IsApprovedPublic = true).
    /// </summary>
    public IEnumerable<Event> ApprovedEvents { get; set; } = new List<Event>();

    /// <summary>
    /// Public events that have been denied (IsPublic = true, IsDeniedPublic = true).
    /// </summary>
    public IEnumerable<Event> DeniedEvents { get; set; } = new List<Event>();
}
