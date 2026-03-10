using CptcEvents.Models;
using CptcEvents.Services;

namespace CptcEvents.Authorization.InviteAuthorizationService;

/// <summary>
/// Authorization service for invite redemption checks.
/// </summary>
public class InviteAuthorizationService : IInviteAuthorizationService
{
    private readonly IGroupService _groupService;

    public InviteAuthorizationService(IGroupService groupService)
    {
        _groupService = groupService;
    }

    /// <inheritdoc/>
    public ServicesAuthorizationResult CanViewRedeem(GroupInvite? invite, string userId)
    {
        if (invite == null)
            return ServicesAuthorizationResult.Fail(AuthorizationFailure.ResourceNotFound);

        if (invite.IsExpired)
            return ServicesAuthorizationResult.Fail(AuthorizationFailure.InviteExpired);

        if (invite.InvitedUserId != null && invite.InvitedUserId != userId)
            return ServicesAuthorizationResult.Fail(AuthorizationFailure.InviteNotForUser);

        return ServicesAuthorizationResult.Success();
    }

    /// <inheritdoc/>
    public async Task<ServicesAuthorizationResult> CanRedeemInviteAsync(GroupInvite invite, string userId)
    {
        var viewCheck = CanViewRedeem(invite, userId);
        if (!viewCheck.Succeeded)
            return viewCheck;

        if (await _groupService.IsUserMemberAsync(invite.GroupId, userId))
            return ServicesAuthorizationResult.Fail(AuthorizationFailure.AlreadyMember);

        return ServicesAuthorizationResult.Success();
    }
}
