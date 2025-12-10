using CptcEvents.Models;

namespace CptcEvents.Application.Mappers;

/// <summary>
/// Maps group EF entities into lightweight view models.
/// Provides projection methods for converting domain models to display models.
/// </summary>
public static class GroupMapper
{
    /// <summary>
    /// Projects a group entity into a minimal summary view model containing only ID and name.
    /// </summary>
    /// <param name="group">The group entity to project.</param>
    /// <returns>A summary view model with basic group information.</returns>
    public static GroupSummaryViewModel ToSummary(Group group) => new()
    {
        Id = group.Id,
        Name = group.Name
    };

    /// <summary>
    /// Projects a group entity into the management dashboard view model with metadata and statistics.
    /// </summary>
    /// <param name="group">The group entity to project.</param>
    /// <param name="userIsOwner">Whether the current user is the group owner.</param>
    /// <param name="userIsModerator">Whether the current user is a moderator.</param>
    /// <param name="moderatorCount">The count of moderators in the group.</param>
    /// <param name="inviteCount">The count of pending invites for the group.</param>
    /// <param name="upcomingEventCount">The count of upcoming events for the group.</param>
    /// <param name="upcomingEvents">The list of upcoming events.</param>
    /// <returns>A management view model with full group details and statistics.</returns>
    public static ManageGroupViewModel ToManageGroup(Group group, bool userIsOwner, bool userIsModerator, int moderatorCount, int inviteCount, int upcomingEventCount, List<GroupEventListItemViewModel> upcomingEvents) => new()
    {
        Group = ToSummary(group),
        Description = group.Description,
        PrivacyLevel = group.PrivacyLevel,
        Color = group.Color,
        UserIsOwner = userIsOwner,
        UserIsModerator = userIsModerator,
        ModeratorsCanInvite = group.PrivacyLevel != PrivacyLevel.OwnerInvitePrivate,
        MemberCount = group.MemberCount,
        ModeratorCount = moderatorCount,
        InviteCount = inviteCount,
        UpcomingEventCount = upcomingEventCount,
        UpcomingEvents = upcomingEvents
    };

    /// <summary>
    /// Projects a group entity into the events view model for displaying group events with user-specific context.
    /// </summary>
    /// <param name="group">The group entity to project.</param>
    /// <param name="userIsModerator">Whether the current user is a moderator.</param>
    /// <param name="userIsOwner">Whether the current user is the group owner.</param>
    /// <param name="upcomingEvents">The list of upcoming events for the overview.</param>
    /// <param name="allEvents">The complete list of events for manage mode.</param>
    /// <returns>A view model for displaying group events with UI context.</returns>
    public static GroupEventsViewModel ToGroupEvents(Group group, bool userIsModerator, bool userIsOwner, List<GroupEventListItemViewModel> upcomingEvents, List<GroupEventListItemViewModel> allEvents) => new()
    {
        Group = ToSummary(group),
        UserIsModerator = userIsModerator,
        UserIsOwner = userIsOwner,
        ModeratorsCanInvite = group.PrivacyLevel != PrivacyLevel.OwnerInvitePrivate,
        UpcomingEvents = upcomingEvents,
        Events = allEvents
    };
}
