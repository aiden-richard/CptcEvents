using System.ComponentModel.DataAnnotations;

namespace CptcEvents.Models;

/// <summary>
/// View model for creating an instructor code.
/// </summary>
public class CreateInstructorCodeViewModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Expires")]
    public bool Expires { get; set; }

    [Display(Name = "Expires At")]
    public DateTime? ExpiresAt { get; set; }
}