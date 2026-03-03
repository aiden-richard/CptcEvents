using Microsoft.AspNetCore.Mvc;

namespace CptcEvents.Authorization;

/// <summary>
/// Extension methods for <see cref="ServicesAuthorizationResult"/> to convert authorization failures into appropriate HTTP responses.
/// </summary>
public static class AuthorizationResultExtensions
{
    /// <summary>
    /// Converts a failed authorization result into an appropriate HTTP action result.
    /// </summary>
    /// <param name="result">The authorization result to convert.</param>
    /// <param name="controller">The controller context for creating action results.</param>
    /// <returns>An action result appropriate for the failure reason (Challenge, NotFound, or Forbid).</returns>
    public static IActionResult ToActionResult(this ServicesAuthorizationResult result, Controller controller)
    {
        return result.Failure switch
        {
            AuthorizationFailure.NotAuthenticated => controller.Challenge(),
            AuthorizationFailure.ResourceNotFound => controller.NotFound(),
            AuthorizationFailure.NotMember => controller.RedirectToAction("Index"),
            AuthorizationFailure.NotModerator => controller.Forbid(),
            AuthorizationFailure.NotOwner => controller.Forbid(),
            _ => controller.Forbid()
        };
    }
}
