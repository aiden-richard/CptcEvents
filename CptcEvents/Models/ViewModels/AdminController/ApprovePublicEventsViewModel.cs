namespace CptcEvents.Models.ViewModels;

/// <summary>
/// View model for the ApprovePublicEvents admin page.
/// Contains paginated pending, approved, and denied public events.
/// </summary>
public class ApprovePublicEventsViewModel
{
    /// <summary>
    /// Paginated events with <see cref="ApprovalStatus.PendingApproval"/> status.
    /// </summary>
    public PagedResult<Event> PendingEvents { get; set; } = new();

    /// <summary>
    /// Paginated events with <see cref="ApprovalStatus.Approved"/> status.
    /// </summary>
    public PagedResult<Event> ApprovedEvents { get; set; } = new();

    /// <summary>
    /// Paginated events with <see cref="ApprovalStatus.Denied"/> status.
    /// </summary>
    public PagedResult<Event> DeniedEvents { get; set; } = new();
}
