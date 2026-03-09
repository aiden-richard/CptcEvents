namespace CptcEvents.Models.ViewModels;

/// <summary>
/// View model for the ApprovePublicEvents admin page.
/// Contains pending, approved, and denied public events.
/// </summary>
public class ApprovePublicEventsViewModel
{
    /// <summary>
    /// Events with <see cref="ApprovalStatus.PendingApproval"/> status.
    /// </summary>
    public IEnumerable<Event> PendingEvents { get; set; } = new List<Event>();

    /// <summary>
    /// Events with <see cref="ApprovalStatus.Approved"/> status.
    /// </summary>
    public IEnumerable<Event> ApprovedEvents { get; set; } = new List<Event>();

    /// <summary>
    /// Events with <see cref="ApprovalStatus.Denied"/> status.
    /// </summary>
    public IEnumerable<Event> DeniedEvents { get; set; } = new List<Event>();
}
