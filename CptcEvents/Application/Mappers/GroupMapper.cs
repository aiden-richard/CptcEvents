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
}
