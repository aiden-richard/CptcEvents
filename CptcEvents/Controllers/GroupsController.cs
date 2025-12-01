using Microsoft.AspNetCore.Mvc;
using CptcEvents.Models;
using CptcEvents.Services;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace CptcEvents.Controllers
{
    public class GroupsController : Controller
    {
        private readonly IGroupService _groupService;
        private readonly IInviteService _inviteService;
        private readonly UserManager<ApplicationUser> _userManager;

        public GroupsController(IGroupService groupService, IInviteService inviteService, UserManager<ApplicationUser> userManager)
        {
            _groupService = groupService;
            _inviteService = inviteService;
            _userManager = userManager;
        }

        [HttpGet("Groups/{groupId?}")]
        public async Task<IActionResult> Index(int? groupId)
        {
            string? userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            IEnumerable<Group> groups = await _groupService.GetGroupsForUserAsync(userId);

            return View(groups);
        }

        // GET: Groups/Create
        [HttpGet("Groups/Create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Groups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupViewModel model)
        {
            string? userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Group newGroup = new Group
            {
                Name = model.Name,
                Description = model.Description,
                PrivacyLevel = model.PrivacyLevel,
                OwnerId = userId
            };

            // Persist the new group to the database
            await _groupService.CreateGroupAsync(newGroup);

            return RedirectToAction(nameof(Index));
        }

        // POST: Groups/Join/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Join(int id)
        {
            // TODO: Add membership record for the current user

            return RedirectToAction(nameof(Index));
        }

        // POST: Groups/Leave/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Leave(int id)
        {
            // TODO: Remove membership record for the current user

            return RedirectToAction(nameof(Index));
        }

        // GET: Groups/Delete/5
        [HttpGet]
        public IActionResult Delete(int id)
        {
            // TODO: Load the group and show confirmation view
            return View();
        }

        // POST: Groups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            // TODO: Delete the group (or mark as deleted)

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Displays the form for creating a new group invite.
        /// GET /Groups/{groupId}/Invites/Create
        /// </summary>
        /// <param name="groupId">The group ID to create an invite for.</param>
        /// <returns>View with invite creation form.</returns>
        [HttpGet("Groups/{groupId}/Invites/Create")]
        public async Task<IActionResult> CreateInvite(int groupId)
        {
            string? userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Forbid();
            }

            return View(new GroupInviteViewModel { GroupId = groupId });
        }

        /// <summary>
        /// Handles the submission of a new group invite, validating authorization and creating the invite.
        /// POST /Invites/Create/{groupId}
        /// </summary>
        /// <param name="groupId">The group ID to create an invite for.</param>
        /// <param name="invite">The invite view model with creation details.</param>
        /// <returns>Redirect to invite details on success, or view with errors on failure.</returns>
        [HttpPost("Groups/{groupId}/Invites/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInvite(int groupId, GroupInviteViewModel invite)
        {
            string? userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            if (groupId != invite.GroupId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(invite);
            }

            // Validate invite creation authorization and business rules via service
            InviteValidationResult validation = await _inviteService.ValidateCreateInviteAsync(userId, invite);

            if (validation.NotFound)
            {
                return NotFound();
            }
            if (validation.Unauthorized)
            {
                return Unauthorized();
            }

            if (!validation.IsValid)
            {
                foreach (KeyValuePair<string, string> kv in validation.FieldErrors)
                {
                    ModelState.AddModelError(kv.Key, kv.Value);
                }
                return View(invite);
            }

            // Create the invite using validated data
            string inviteCode = await _inviteService.GenerateUniqueInviteCodeAsync(8);
            DateTime? expiresAt = validation.ValidatedExpiresAt;

            GroupInvite newInvite = new GroupInvite
            {
                GroupId = invite.GroupId,
                CreatedById = userId,
                InvitedUserId = validation.InvitedUserId,
                InviteCode = inviteCode,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                OneTimeUse = invite.OneTimeUse,
                IsUsed = false,
                TimesUsed = 0
            };

            newInvite = await _inviteService.CreateInviteAsync(newInvite);

            //return Redirect($"/Groups/{newInvite.GroupId}/Invites/{newInvite.Id}");
            return RedirectToAction(nameof(InviteDetails), new { groupId = newInvite.GroupId, inviteId = newInvite.Id });
        }

        /// <summary>
        /// Displays detailed information about a specific invite.
        /// GET /Groups/{groupId}/Invites/{inviteId}
        /// </summary>
        /// <param name="id">The invite ID to display.</param>
        /// <returns>View with invite details, or NotFound if invite doesn't exist.</returns>
        [HttpGet("Groups/{groupId}/Invites/{inviteId}")]
        public async Task<IActionResult> InviteDetails(int groupId, int inviteId)
        {
            string? userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Forbid();
            }

            GroupInvite? invite = await _inviteService.GetInviteAsync(inviteId);
            if (invite == null || invite.CreatedById != userId)
            {
                return NotFound();
            }

            // if the invite is user-specific and the current user is the invited user, redirect to redeem page
            if (invite.InvitedUserId != null && invite.InvitedUserId == userId)
            {
                return RedirectToAction(nameof(InvitesController.Redeem), "Invites", new { code = invite.InviteCode });
            }

            return View(invite);
        }
    }
}
