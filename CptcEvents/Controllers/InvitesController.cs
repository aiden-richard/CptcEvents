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

        /// <summary>
        /// Creates a new instance of <see cref="InvitesController"/>.
        /// </summary>
        /// <param name="inviteService">Invite service for data operations.</param>
        /// <param name="userManager">User manager for identity operations.</param>
        public InvitesController(IInviteService inviteService, UserManager<ApplicationUser> userManager)
        {
            _inviteService = inviteService;
            _userManager = userManager;
        }

        /// <summary>
        /// Displays the invite redemption page where users can join a group.
        /// GET /Invites/Redeem/{code}
        /// </summary>
        /// <param name="code">The invite code to redeem.</param>
        /// <returns>View with redemption details, NotFound if invalid, or Unauthorized if user-specific.</returns>
        [HttpGet("/Invites/Redeem/{code}")]
        public async Task<IActionResult> Redeem(string code)
        {
            string? userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Forbid();
            }

            GroupInvite? invite = await _inviteService.GetInviteByCodeAsync(code);

            if (invite == null || (invite.OneTimeUse && invite.IsUsed) || (invite.ExpiresAt != null && invite.ExpiresAt < DateTime.UtcNow))
            {
                return NotFound();
            }

            if (invite.InvitedUserId != null && invite.InvitedUserId != userId)
            {
                return Unauthorized();
            }

            return View(invite);
        }

        /// <summary>
        /// Processes the redemption of an invite, adding the user to the group if valid.
        /// POST /Invites/Redeem/{code}
        /// </summary>
        /// <param name="code">The invite code to redeem.</param>
        /// <returns>Redirect to group details on success, or view with errors on failure.</returns>
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
                ModelState.AddModelError(string.Empty, "The invite could not be found.");
                return NotFound();
            }

            if ((invite.OneTimeUse && invite.IsUsed) || (invite.ExpiresAt != null && invite.ExpiresAt < DateTime.UtcNow))
            {
                ModelState.AddModelError(string.Empty, "This invite is expired or already used.");
                return View(invite);
            }

            if (invite.InvitedUserId != null && invite.InvitedUserId != userId)
            {
                return Unauthorized();
            }

            GroupMember? member = await _inviteService.RedeemInviteAsync(invite.Id, userId);
            if (member == null)
            {
                ModelState.AddModelError(string.Empty, "Could not redeem the invite. You may already be a member or the invite is no longer valid.");
            // Re-fetch to ensure view has current state
            invite = await _inviteService.GetInviteByCodeAsync(code);
            return View(invite);
        }

        return RedirectToAction(nameof(GroupsController.Index), "Groups");
    }
    }
}