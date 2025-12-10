using CptcEvents.Models;

namespace CptcEvents.Application.Mappers;

/// <summary>
/// Maps between event EF entities and view/DTO models.
/// </summary>
public static class EventMapper
{
    /// <summary>
    /// Builds a full event view model for details and list scenarios.
    /// </summary>
    /// <param name="e">Event entity from the database.</param>
    public static EventDetailsViewModel ToDetails(Event e) => new()
    {
        Id = e.Id,
        Title = e.Title,
        Description = e.Description,
        GroupName = e.Group?.Name,
        GroupId = e.GroupId,
        DateOfEvent = e.DateOfEvent,
        StartTime = e.StartTime,
        EndTime = e.EndTime,
        IsAllDay = e.IsAllDay,
        IsPublic = e.IsPublic,
        IsApprovedPublic = e.IsApprovedPublic,
        IsDeniedPublic = e.IsDeniedPublic,
        Url = e.Url,
        IsCurrentUserMember = false // Default value when membership status is not provided
    };

    /// <summary>
    /// Builds a full event view model for details and list scenarios with user membership info.
    /// </summary>
    /// <param name="e">Event entity from the database.</param>
    /// <param name="isCurrentUserMember">Whether the current user is a member of the event's group.</param>
    public static EventDetailsViewModel ToDetails(Event e, bool isCurrentUserMember) => new()
    {
        Id = e.Id,
        Title = e.Title,
        Description = e.Description,
        GroupName = e.Group?.Name,
        GroupId = e.GroupId,
        DateOfEvent = e.DateOfEvent,
        StartTime = e.StartTime,
        EndTime = e.EndTime,
        IsAllDay = e.IsAllDay,
        IsPublic = e.IsPublic,
        IsApprovedPublic = e.IsApprovedPublic,
        IsDeniedPublic = e.IsDeniedPublic,
        Url = e.Url,
        IsCurrentUserMember = isCurrentUserMember
    };

    /// <summary>
    /// Shapes an event into a compact row for group event listings.
    /// </summary>
    /// <param name="e">Event entity for a group.</param>
    public static GroupEventListItemViewModel ToGroupEventListItem(Event e) => new()
    {
        Id = e.Id,
        Title = e.Title,
        DateOfEvent = e.DateOfEvent,
        StartTime = e.StartTime,
        EndTime = e.EndTime,
        IsAllDay = e.IsAllDay,
        IsPublic = e.IsPublic,
        IsApprovedPublic = e.IsApprovedPublic,
        IsDeniedPublic = e.IsDeniedPublic,
        Url = e.Url,
        CreatedByUserId = e.CreatedByUserId
    };

    /// <summary>
    /// Creates a new event entity from the create/edit event form input.
    /// </summary>
    /// <param name="model">User-submitted event form model.</param>
    public static Event ToEntity(EventFormViewModel model) => new()
    {
        Title = model.Title,
        Description = model.Description,
        GroupId = model.GroupId,
        IsPublic = model.IsPublic,
        IsAllDay = model.IsAllDay,
        DateOfEvent = model.DateOfEvent,
        StartTime = model.StartTime,
        EndTime = model.EndTime,
        Url = model.Url
    };

    /// <summary>
    /// Applies edits from the event form onto an event instance while keeping immutable fields unchanged.
    /// </summary>
    /// <param name="model">User-submitted event form model.</param>
    /// <param name="target">The event entity to mutate before persistence.</param>
    public static void ApplyUpdates(EventFormViewModel model, Event target)
    {
        target.Title = model.Title;
        target.Description = model.Description;
        target.IsPublic = model.IsPublic;
        target.IsAllDay = model.IsAllDay;
        target.DateOfEvent = model.DateOfEvent;
        target.StartTime = model.StartTime;
        target.EndTime = model.EndTime;
        target.Url = model.Url;
    }

    /// <summary>
    /// Projects an event into the FullCalendar DTO used by the calendar UI.
    /// </summary>
    /// <param name="e">Event entity to render on the calendar.</param>
    public static FullCalendarEventDto ToFullCalendarEvent(Event e)
    {
        string start;
        string end;

        if (e.IsAllDay)
        {
            start = e.DateOfEvent.ToString("yyyy-MM-dd");
            end = e.DateOfEvent.AddDays(1).ToString("yyyy-MM-dd");
        }
        else
        {
            start = e.DateOfEvent.ToDateTime(e.StartTime).ToString("s");
            end = e.DateOfEvent.ToDateTime(e.EndTime).ToString("s");
        }

        // Use group's custom color if available, otherwise generate a deterministic color based on group ID
        string color = !string.IsNullOrEmpty(e.Group?.Color) 
            ? e.Group.Color 
            : GenerateColorForGroup(e.GroupId);

        return new FullCalendarEventDto
        {
            Id = e.Id,
            Title = e.Title,
            Start = start,
            End = end,
            AllDay = e.IsAllDay,
            GroupId = e.GroupId,
            BackgroundColor = color
        };
    }

    /// <summary>
    /// Generates a consistent pseudo-random color for a group based on its ID.
    /// The same GroupId will always produce the same color.
    /// </summary>
    /// <param name="groupId">The group identifier.</param>
    /// <returns>A hex color string.</returns>
    private static string GenerateColorForGroup(int groupId)
    {
        // List of distinct, visually appealing colors
        string[] colors = new[]
        {
            "#0d6efd", // Blue
            "#198754", // Green
            "#dc3545", // Red
            "#ffc107", // Yellow
            "#17a2b8", // Cyan
            "#e83e8c", // Pink
            "#fd7e14", // Orange
            "#6610f2", // Indigo
            "#6f42c1", // Purple
            "#20c997", // Teal
            "#ff6b6b", // Coral
            "#4ecdc4", // Turquoise
            "#45b7d1", // Sky Blue
            "#96ceb4", // Sage
            "#ffeaa7"  // Light Yellow
        };

        // Use groupId modulo to select a color deterministically
        int colorIndex = Math.Abs(groupId) % colors.Length;
        return colors[colorIndex];
    }
}
