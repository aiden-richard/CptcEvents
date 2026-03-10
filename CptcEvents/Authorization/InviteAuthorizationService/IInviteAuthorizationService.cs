using CptcEvents.Models;

namespace CptcEvents.Authorization.InviteAuthorizationService;

/// <summary>
/// Encapsulates authorization checks for invite redemption.
/// </summary>
public interface IInviteAuthorizationService
{
    /// <summary>
    /// Checks whether the given user can view the invite redemption page.
    /// Verifies the invite exists, is not expired, and is not restricted to a different user.
    /// </summary>
    /// <param name="invite">The invite to check, or null if not found.</param>
    /// <param name="userId">The ID of the current user.</param>
    ServicesAuthorizationResult CanViewRedeem(GroupInvite? invite, string userId);

    /// <summary>
    /// Checks whether the given user can complete redeeming the invite (join the group).
    /// Performs all <see cref="CanViewRedeem"/> checks and additionally ensures the user
    /// is not already a member of the group.
    /// </summary>
    /// <param name="invite">The invite to redeem.</param>
    /// <param name="userId">The ID of the current user.</param>
    Task<ServicesAuthorizationResult> CanRedeemInviteAsync(GroupInvite invite, string userId);
}
