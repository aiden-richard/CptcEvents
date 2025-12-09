using CptcEvents.Models;

namespace CptcEvents.Application.Mappers;

/// <summary>
/// Maps group EF entities into lightweight view models.
/// </summary>
public static class GroupMapper
{
    public static GroupSummaryViewModel ToSummary(Group group) => new()
    {
        Id = group.Id,
        Name = group.Name
    };
}
