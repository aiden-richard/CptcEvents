using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace CptcEvents.Models;

/// <summary>
/// Represents an application user with additional profile information.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Gets or sets the first name of the person.
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// Gets or sets the last name of the person.
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// Gets or sets the URL of the user's profile picture.
    /// </summary>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>
    /// Groups that this user has joined. Many-to-many relationship with Group.Members.
    /// </summary>
    public List<Group> JoinedGroups { get; set; } = new();
}
