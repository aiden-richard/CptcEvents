namespace CptcEvents.Models;

/// <summary>
/// View model for displaying error information to the user.
/// </summary>
public class ErrorViewModel
{
    /// <summary>
    /// Gets or sets the HTTP request ID for error tracking and logging.
    /// </summary>
    public string? RequestId { get; set; }

    /// <summary>
    /// Gets a value indicating whether the request ID should be displayed to the user.
    /// </summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
