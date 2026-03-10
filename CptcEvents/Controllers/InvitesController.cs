using CptcEvents.Authorization;
using CptcEvents.Authorization.InviteAuthorizationService;
using AuthorizationFailure = CptcEvents.Authorization.AuthorizationFailure;
using CptcEvents.Models;
using CptcEvents.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace CptcEvents.Controllers
{
    /// <summary>
    /// Controller for managing group invites, including creation, viewing, and redemption.
    /// Requires authentication for all actions.
    /// </summary>
    [Authorize]
    public class InvitesController : Controller
    {
        private readonly IInviteService _inviteService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IInviteAuthorizationService _inviteAuthorization;

        /// <summary>
        /// Creates a new instance of <see cref="InvitesController"/>.
        /// </summary>
        /// <param name="inviteService">Invite service for data operations.</param>
        /// <param name="userManager">User manager for identity operations.</param>
        /// <param name="inviteAuthorization">Invite authorization service.</param>
        public InvitesController(IInviteService inviteService, UserManager<ApplicationUser> userManager, IInviteAuthorizationService inviteAuthorization)
        {
            _inviteService = inviteService;
            _userManager = userManager;
            _inviteAuthorization = inviteAuthorization;
        }

        /// <summary>
        /// Handles invite redemption when the code is provided as a query string.
        /// Redirects to the route variant or back to Join when missing.
        /// GET /Invites/Redeem?code={code}
        /// </summary>
        /// <param name="inviteCode">Invite code supplied via query string.</param>
        /// <returns>Redirects to Redeem action with code parameter or to Groups.Join if no code provided.</returns>
        [HttpGet("/Invites/Redeem")]
        public IActionResult RedeemByQuery([FromQuery(Name = "code")] string? inviteCode)
        {
            if (string.IsNullOrWhiteSpace(inviteCode))
            {
                return RedirectToAction(nameof(GroupsController.Join), "Groups");
            }

            return RedirectToAction(nameof(Redeem), new { code = inviteCode });
        }

        /// <summary>
        /// Displays the invite redemption page where users can join a group.
        /// GET /Invites/Redeem/{code}
        /// </summary>
        /// <param name="code">The invite code to redeem.</param>
        /// <returns>View with redemption details, or redirects if invalid, expired, or user-specific mismatch.</returns>
        [HttpGet("/Invites/Redeem/{code}")]
        public async Task<IActionResult> Redeem(string code)
        {
            string? userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Forbid();
            }

            GroupInvite? invite = await _inviteService.GetInviteByCodeAsync(code);

            ServicesAuthorizationResult authCheck = _inviteAuthorization.CanViewRedeem(invite, userId);
            if (!authCheck.Succeeded)
            {
                return authCheck.Failure == AuthorizationFailure.InviteNotForUser
                    ? Unauthorized()
                    : RedirectToAction(nameof(RedeemByQuery));
            }

            return View(invite);
        }

        /// <summary>
        /// Processes the redemption of an invite, adding the user to the group if valid.
        /// POST /Invites/Redeem/{code}
        /// </summary>
        /// <param name="code">The invite code to redeem.</param>
        /// <returns>Redirects to Groups.Index on success, or view with errors on failure.</returns>
        [HttpPost("/Invites/Redeem/{code}")]
        [ValidateAntiForgeryToken]
        [ActionName("Redeem")]
        public async Task<IActionResult> RedeemConfirmed(string code)
        {
            string? userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Forbid();
            }

            GroupInvite? invite = await _inviteService.GetInviteByCodeAsync(code);

            if (invite == null)
            {
                return NotFound();
            }

            ServicesAuthorizationResult authCheck = await _inviteAuthorization.CanRedeemInviteAsync(invite, userId);
            if (!authCheck.Succeeded)
            {
                switch (authCheck.Failure)
                {
                    case AuthorizationFailure.InviteExpired:
                        ModelState.AddModelError(string.Empty, "This invite is expired or already used.");
                        return View(invite);
                    case AuthorizationFailure.InviteNotForUser:
                        return Unauthorized();
                    case AuthorizationFailure.AlreadyMember:
                        ViewData["Error"] = "You are already a member of this group.";
                        return View(invite);
                    default:
                        return Forbid();
                }
            }

            GroupMember? member = await _inviteService.RedeemInviteAsync(invite.Id, userId);
            if (member == null)
            {
                ModelState.AddModelError(string.Empty, "Could not redeem the invite. You may already be a member or the invite is no longer valid.");
                invite = await _inviteService.GetInviteByCodeAsync(code);
                return View(invite);
            }

            return RedirectToAction(nameof(GroupsController.Index), "Groups");
        }
    }
}
