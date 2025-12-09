namespace CptcEvents.Models;

/// <summary>
/// Minimal read model for displaying group metadata alongside events.
/// </summary>
public class GroupSummaryViewModel
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;
}
