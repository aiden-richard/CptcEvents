using Microsoft.AspNetCore.Mvc;
using CptcEvents.Models;
using CptcEvents.Services;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace CptcEvents.Controllers
{
    [Authorize]
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

        #region Group CRUD Operations

        // GET: Groups or Groups/{groupId}
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

        // GET: Groups/Edit/5
        [HttpGet("Groups/Edit/{groupId}")]
        [Authorize(Policy = "GroupModerator")]
        public async Task<IActionResult> Edit(int groupId)
        {
            string? userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            Group? group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }

            bool isOwner = await _groupService.IsUserOwnerAsync(groupId, userId);

            var viewModel = new GroupEditViewModel
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                PrivacyLevel = group.PrivacyLevel,
                IsOwner = isOwner
            };

            return View(viewModel);
        }

        // POST: Groups/Edit/5
        [HttpPost("Groups/Edit/{groupId}")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "GroupModerator")]
        public async Task<IActionResult> Edit(int groupId, GroupEditViewModel model)
        {
            string? userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            if (groupId != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                model.IsOwner = await _groupService.IsUserOwnerAsync(groupId, userId);
                return View(model);
            }

            Group? existingGroup = await _groupService.GetGroupByIdAsync(groupId);
            if (existingGroup == null)
            {
                return NotFound();
            }

            // Update group properties
            existingGroup.Name = model.Name;
            existingGroup.Description = model.Description;
            existingGroup.PrivacyLevel = model.PrivacyLevel;

            Group? result = await _groupService.UpdateGroupAsync(existingGroup, userId);
            if (result == null)
            {
                ModelState.AddModelError(string.Empty, "Failed to update group. You may not have permission to make these changes.");
                model.IsOwner = await _groupService.IsUserOwnerAsync(groupId, userId);
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Groups/Delete/5
        [HttpGet("Groups/Delete/{groupId}")]
        [Authorize(Policy = "GroupOwner")]
        public async Task<IActionResult> Delete(int groupId)
        {
            Group? group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }

            return View(group);
        }

        // POST: Groups/Delete/5
        [HttpPost("Groups/Delete/{groupId}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "GroupOwner")]
        public async Task<IActionResult> DeleteConfirmed(int groupId)
        {
            Group? group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }

            await _groupService.DeleteGroupAsync(groupId);

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Membership Operations

        // GET: Groups/Leave/5
        [HttpGet("Groups/Leave/{groupId}")]
        [Authorize(Policy = "GroupMember")]
        public async Task<IActionResult> Leave(int groupId)
        {
            string? userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            Group? group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }

            // Check if user is a member, need to be a member to leave
            bool isMember = await _groupService.IsUserMemberAsync(groupId, userId);
            if (isMember == false)
            {
                return Forbid();
            }

            // Check if user is the owner, owners cannot leave their own group
            bool isOwner = await _groupService.IsUserOwnerAsync(groupId, userId);
            ViewBag.IsOwner = isOwner;

            return View(group);
        }

        // POST: Groups/Leave/5
        [HttpPost("Groups/Leave/{groupId}")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "GroupMember")]
        public async Task<IActionResult> LeaveConfirmed(int groupId)
        {
            string? userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            Group? group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }

            // Check if user is a member, need to be a member to leave
            bool isMember = await _groupService.IsUserMemberAsync(groupId, userId);
            if (isMember == false)
            {
                return NotFound();
            }

            // Check if user is the owner, owners cannot leave their own group
            bool isOwner = await _groupService.IsUserOwnerAsync(groupId, userId);
            if (isOwner)
            {
                ViewBag.IsOwner = true;
                ModelState.AddModelError(string.Empty, "Group owners cannot leave their own group. Please transfer ownership or delete the group.");
                return View("Leave", group);
            }

            await _groupService.RemoveUserFromGroupAsync(groupId, userId);

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Invite Operations

        /// <summary>
        /// Displays the form for creating a new group invite.
        /// GET /Groups/{groupId}/Invites/Create
        /// </summary>
        /// <param name="groupId">The group ID to create an invite for.</param>
        /// <returns>View with invite creation form.</returns>
        [HttpGet("Groups/{groupId}/Invites/Create")]
        [Authorize(Policy = "GroupModerator")]
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
        /// POST /Groups/{groupId}/Invites/Create
        /// </summary>
        /// <param name="groupId">The group ID to create an invite for.</param>
        /// <param name="invite">The invite view model with creation details.</param>
        /// <returns>Redirect to invite details on success, or view with errors on failure.</returns>
        [HttpPost("Groups/{groupId}/Invites/Create")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "GroupModerator")]
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
                TimesUsed = 0
            };

            newInvite = await _inviteService.CreateInviteAsync(newInvite);

            return RedirectToAction(nameof(InviteDetails), new { groupId = newInvite.GroupId, inviteId = newInvite.Id });
        }

        /// <summary>
        /// Displays detailed information about a specific invite.
        /// GET /Groups/{groupId}/Invites/{inviteId}
        /// </summary>
        /// <param name="groupId">The group ID the invite belongs to.</param>
        /// <param name="inviteId">The invite ID to display.</param>
        /// <returns>View with invite details, or NotFound if invite doesn't exist.</returns>
        [HttpGet("Groups/{groupId}/Invites/{inviteId}")]
        [Authorize(Policy = "GroupModerator")]
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

            // If the invite is user-specific and the current user is the invited user, redirect to redeem page
            if (invite.InvitedUserId != null && invite.InvitedUserId == userId)
            {
                return RedirectToAction(nameof(InvitesController.Redeem), "Invites", new { code = invite.InviteCode });
            }

            return View(invite);
        }

        #endregion
    }
}
