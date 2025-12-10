using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CptcEvents.Models;

/// <summary>
/// Represents an application user with additional profile information.
/// Extends <see cref="IdentityUser"/> with custom properties for the application.
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Gets or sets the first name of the person.
    /// </summary>
    [Required]
    [StringLength(50)]
    public required string FirstName { get; set; }

    /// <summary>
    /// Gets or sets the last name of the person.
    /// </summary>
    [Required]
    [StringLength(50)]
    public required string LastName { get; set; }

    /// <summary>
    /// Gets or sets the URL of the user's profile picture.
    /// </summary>
    [MaxLength(500)]
    public string? ProfilePictureUrl { get; set; }

    /// <summary>
    /// Gets or sets the collection of group memberships for this user.
    /// Represents the many-to-many relationship with groups through <see cref="GroupMember"/>.
    /// </summary>
    public ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();
}
