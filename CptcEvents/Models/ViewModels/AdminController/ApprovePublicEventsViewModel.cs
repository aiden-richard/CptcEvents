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
    public List<Event> PendingEvents { get; set; } = new();

    /// <summary>
    /// Public events that have been approved (IsPublic = true, IsApprovedPublic = true).
    /// </summary>
    public List<Event> ApprovedEvents { get; set; } = new();

    /// <summary>
    /// Public events that have been denied (IsPublic = true, IsDeniedPublic = true).
    /// </summary>
    public List<Event> DeniedEvents { get; set; } = new();
}
