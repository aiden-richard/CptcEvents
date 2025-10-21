using System.ComponentModel.DataAnnotations;

namespace CptcEventHub.Models;

/// <summary>
/// This class represents an event in the website.
/// It is used to store information about events such as title, description, date, and location.
/// Some events are public and some are private.
/// </summary>
public class Event
{
    [Key]
    public Guid Guid { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
}
