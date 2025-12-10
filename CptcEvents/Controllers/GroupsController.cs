using CptcEvents.Application.Mappers;
using CptcEvents.Authorization;
using CptcEvents.Models;
using CptcEvents.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CptcEvents.Controllers
{
    [Authorize]
    public class GroupsController : Controller
    {
        private readonly IGroupService _groupService;
        private readonly IInviteService _inviteService;
        private readonly IEventService _eventService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGroupAuthorizationService _groupAuthorization;

        public GroupsController(IGroupService groupService, IInviteService inviteService, IEventService eventService, IGroupAuthorizationService groupAuthorization, UserManager<ApplicationUser> userManager)
        {
            _groupService = groupService;
            _inviteService = inviteService;
            _eventService = eventService;
            _groupAuthorization = groupAuthorization;
            _userManager = userManager;
        }

        #region Groups

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
        public async Task<IActionResult> Create(GroupFormViewModel model)
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
        [Authorize(Policy = "GroupOwner")]
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

            var viewModel = new GroupFormViewModel
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
        [Authorize(Policy = "GroupOwner")]
        public async Task<IActionResult> Edit(int groupId, GroupFormViewModel model)
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

            return RedirectToAction(nameof(ManageGroup), new { groupId = groupId });
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

        /// <summary>
        /// Displays the group management dashboard.
        /// </summary>
        /// <param name="groupId">The ID of the group to manage.</param>
        /// <returns>View with management options.</returns>
        [HttpGet("Groups/Manage/{groupId}")]
        [Authorize(Policy = "GroupModerator")]
        public async Task<IActionResult> ManageGroup(int groupId)
        {
            GroupAuthorizationResult moderatorCheck = await _groupAuthorization.EnsureModeratorAsync(groupId, User);
            if (!moderatorCheck.Succeeded)
            {
                return moderatorCheck.ToActionResult(this);
            }

            string? userId = await _groupAuthorization.GetUserIdAsync(User);
            Group? group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }

            bool isOwner = userId != null && await _groupService.IsUserOwnerAsync(groupId, userId);
            bool moderatorsCanInvite = group.PrivacyLevel != PrivacyLevel.OwnerInvitePrivate;

            List<Event> events = (await _eventService.GetEventsForGroupAsync(groupId)).ToList();
            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

            List<GroupEventListItemViewModel> upcomingEvents = events
                .Where(e => e.DateOfEvent >= today)
                .OrderBy(e => e.DateOfEvent)
                .ThenBy(e => e.IsAllDay ? TimeOnly.MinValue : e.StartTime)
                .Take(5)
                .Select(EventMapper.ToGroupEventListItem)
                .ToList();

            List<GroupInvite> invites = await _inviteService.GetInvitesForGroupAsync(groupId);

            var viewModel = new ManageGroupViewModel
            {
                Group = GroupMapper.ToSummary(group),
                Description = group.Description,
                PrivacyLevel = group.PrivacyLevel,
                UserIsOwner = isOwner,
                UserIsModerator = true,
                ModeratorsCanInvite = moderatorsCanInvite,
                MemberCount = group.Members.Count,
                ModeratorCount = group.Members.Count(m => m.Role == RoleType.Moderator || m.Role == RoleType.Owner),
                InviteCount = invites.Count,
                UpcomingEventCount = upcomingEvents.Count,
                UpcomingEvents = upcomingEvents
            };

            return View(viewModel);
        }

        #endregion

        #region Membership Operations

        // GET: Groups/{groupId}/Join
        [HttpGet("Groups/Join/{groupId?}")]
        [HttpGet("Groups/Join")]
        public async Task<IActionResult> Join(int? groupId)
        {
            string? userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            if (groupId == null)
            {
                return View(model: null);
            }

            Group? group = await _groupService.GetGroupByIdAsync(groupId.Value);
            if (group == null)
            {
                return NotFound();
            }

            bool isMember = await _groupService.IsUserMemberAsync(groupId.Value, userId);
            if (isMember)
            {
                TempData["Info"] = "You are already a member of this group.";
                return RedirectToAction(nameof(Events), new { groupId = groupId });
            }

            if (group.PrivacyLevel != PrivacyLevel.Public)
            {
                return NotFound();
            }

            return View(group);
        }

        // POST: Groups/Join
        [HttpPost("Groups/Join/{groupId}")]
        [ValidateAntiForgeryToken]
        [ActionName("Join")]
        public async Task<IActionResult> JoinConfirmed(int groupId)
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

            bool isMember = await _groupService.IsUserMemberAsync(groupId, userId);
            if (isMember)
            {
                TempData["Info"] = "You are already a member of this group.";
                return RedirectToAction(nameof(Events), new { groupId = groupId });
            }

            if (group.PrivacyLevel != PrivacyLevel.Public)
            {
                return NotFound();
            }

            GroupMember? newMember = await _groupService.AddUserToGroupAsync(groupId, userId, RoleType.Member, null);
            if (newMember == null)
            {
                TempData["Error"] = "Unable to join the group. Please try again or contact a moderator.";
                return RedirectToAction(nameof(Join), new { groupId = groupId });
            }

            return RedirectToAction(nameof(Events), new { groupId = groupId });
        }

        // GET: Groups/Leave/5
        [HttpGet("Groups/Leave/{groupId}")]
        [Authorize(Policy = "GroupMember")]
        public async Task<IActionResult> Leave(int groupId)
        {
            GroupAuthorizationResult memberCheck = await _groupAuthorization.EnsureMemberAsync(groupId, User);
            if (!memberCheck.Succeeded)
            {
                return memberCheck.ToActionResult(this);
            }

            string? userId = await _groupAuthorization.GetUserIdAsync(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            Group? group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }

            bool isOwner = userId != null && await _groupService.IsUserOwnerAsync(groupId, userId);
            ViewBag.IsOwner = isOwner;

            return View(group);
        }

        // POST: Groups/Leave/5
        [HttpPost("Groups/Leave/{groupId}")]
        [ActionName("Leave")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "GroupMember")]
        public async Task<IActionResult> LeaveConfirmed(int groupId)
        {
            GroupAuthorizationResult memberCheck = await _groupAuthorization.EnsureMemberAsync(groupId, User);
            if (!memberCheck.Succeeded)
            {
                return memberCheck.ToActionResult(this);
            }

            string? userId = await _groupAuthorization.GetUserIdAsync(User);

            Group? group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            bool isOwner = await _groupService.IsUserOwnerAsync(groupId, userId);
            if (isOwner)
            {
                ViewBag.IsOwner = true;
                ModelState.AddModelError(string.Empty, "Group owners cannot leave their own group. Please transfer ownership or delete the group.");
                return View(group);
            }

            await _groupService.RemoveUserFromGroupAsync(groupId, userId);

            return RedirectToAction(nameof(Index));
        }

        // GET: Groups/{groupId}/Members
        [HttpGet("Groups/{groupId}/Members")]
        [Authorize(Policy = "GroupOwner")]
        public async Task<IActionResult> ManageMembers(int groupId)
        {
            GroupAuthorizationResult ownerCheck = await _groupAuthorization.EnsureOwnerAsync(groupId, User);
            if (!ownerCheck.Succeeded)
            {
                return ownerCheck.ToActionResult(this);
            }

            string? currentUserId = await _groupAuthorization.GetUserIdAsync(User);
            if (currentUserId == null)
            {
                return Challenge();
            }

            Group? group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }

            List<ManageMemberListItemViewModel> members = group.Members
                .OrderByDescending(m => m.Role == RoleType.Owner)
                .ThenByDescending(m => m.Role == RoleType.Moderator)
                .ThenBy(m => m.User.UserName)
                .Select(m => new ManageMemberListItemViewModel
                {
                    UserId = m.UserId,
                    DisplayName = m.User.UserName ?? m.User.Email ?? m.UserId,
                    UserName = m.User.UserName ?? m.User.Email ?? m.UserId,
                    Role = m.Role,
                    JoinedAt = m.JoinedAt,
                    IsCurrentUser = m.UserId == currentUserId,
                    CanPromote = m.Role == RoleType.Member,
                    CanDemote = m.Role == RoleType.Moderator,
                    CanRemove = m.Role != RoleType.Owner && m.UserId != currentUserId
                })
                .ToList();

            var viewModel = new ManageMembersViewModel
            {
                Group = GroupMapper.ToSummary(group),
                UserIsOwner = true,
                Members = members
            };

            return View(viewModel);
        }

        // POST: Groups/{groupId}/Members/{userId}/Role
        [HttpPost("Groups/{groupId}/Members/{userId}/Role")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "GroupOwner")]
        public async Task<IActionResult> UpdateMemberRole(int groupId, string userId, RoleType newRole)
        {
            GroupAuthorizationResult ownerCheck = await _groupAuthorization.EnsureOwnerAsync(groupId, User);
            if (!ownerCheck.Succeeded)
            {
                return ownerCheck.ToActionResult(this);
            }

            string? currentUserId = await _groupAuthorization.GetUserIdAsync(User);
            if (currentUserId == null)
            {
                return Challenge();
            }

            if (userId == currentUserId)
            {
                TempData["Error"] = "You cannot change your own role.";
                return RedirectToAction(nameof(ManageMembers), new { groupId });
            }

            if (newRole == RoleType.Owner)
            {
                TempData["Error"] = "Ownership transfers are not supported here.";
                return RedirectToAction(nameof(ManageMembers), new { groupId });
            }

            GroupMember? updated = await _groupService.UpdateUserRoleAsync(groupId, userId, newRole);
            if (updated == null)
            {
                TempData["Error"] = "Unable to change the member's role.";
            }
            else
            {
                TempData["Success"] = "Member role updated.";
            }

            return RedirectToAction(nameof(ManageMembers), new { groupId });
        }

        // POST: Groups/{groupId}/Members/{userId}/Remove
        [HttpPost("Groups/{groupId}/Members/{userId}/Remove")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "GroupOwner")]
        public async Task<IActionResult> RemoveMember(int groupId, string userId)
        {
            GroupAuthorizationResult ownerCheck = await _groupAuthorization.EnsureOwnerAsync(groupId, User);
            if (!ownerCheck.Succeeded)
            {
                return ownerCheck.ToActionResult(this);
            }

            string? currentUserId = await _groupAuthorization.GetUserIdAsync(User);
            if (currentUserId == null)
            {
                return Challenge();
            }

            if (userId == currentUserId)
            {
                TempData["Error"] = "You cannot remove yourself from your own group.";
                return RedirectToAction(nameof(ManageMembers), new { groupId });
            }

            Group? group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }

            GroupMember? member = group.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null)
            {
                TempData["Error"] = "Member not found.";
                return RedirectToAction(nameof(ManageMembers), new { groupId });
            }

            if (member.Role == RoleType.Owner)
            {
                TempData["Error"] = "Cannot remove the group owner.";
                return RedirectToAction(nameof(ManageMembers), new { groupId });
            }

            await _groupService.RemoveUserFromGroupAsync(groupId, userId);
            TempData["Success"] = "Member removed from the group.";

            return RedirectToAction(nameof(ManageMembers), new { groupId });
        }

        #endregion

        #region Event Operations

        // GET: Groups/5/Events
        [HttpGet("Groups/{groupId}/Events")]
        [ActionName("Events")]
        [Authorize(Policy = "GroupMember")]
        public async Task<IActionResult> Events(int groupId)
        {
            GroupAuthorizationResult memberCheck = await _groupAuthorization.EnsureMemberAsync(groupId, User);
            if (!memberCheck.Succeeded)
            {
                return memberCheck.ToActionResult(this);
            }

            Group? group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }

            string? userId = await _groupAuthorization.GetUserIdAsync(User);

            bool isModerator =
                (userId != null && await _groupService.IsUserModeratorAsync(groupId, userId))
                || (userId != null && await _groupService.IsUserOwnerAsync(groupId, userId));

            DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
            List<Event> upcomingEvents = (await _eventService.GetEventsForGroupAsync(groupId))
                .Where(e => e.DateOfEvent >= today)
                .OrderBy(e => e.DateOfEvent)
                .ThenBy(e => e.IsAllDay ? TimeOnly.MinValue : e.StartTime)
                .Take(10)
                .ToList();

            var viewModel = new GroupEventsViewModel
            {
                Group = GroupMapper.ToSummary(group),
                UserIsModerator = isModerator,
                UpcomingEvents = upcomingEvents
                    .Select(EventMapper.ToGroupEventListItem)
                    .ToList()
            };

            return View(viewModel);
        }

        // GET: Groups/{groupId}/ManageEvents
        [HttpGet("Groups/{groupId}/ManageEvents")]
        [Authorize(Policy = "GroupModerator")]
        public async Task<IActionResult> ManageEvents(int groupId)
        {
            GroupAuthorizationResult moderatorCheck = await _groupAuthorization.EnsureModeratorAsync(groupId, User);
            if (!moderatorCheck.Succeeded)
            {
                return moderatorCheck.ToActionResult(this);
            }

            Group? group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }

            IEnumerable<Event> events = await _eventService.GetEventsForGroupAsync(groupId);

            var viewModel = new GroupEventsViewModel
            {
                Group = GroupMapper.ToSummary(group),
                UserIsModerator = true,
                Events = events
                    .Select(EventMapper.ToGroupEventListItem)
                    .ToList()
            };

            return View(viewModel);
        }

        #endregion

        #region Invite Operations

        /// <summary>
        /// Lists invites for a group so moderators can manage or revoke them.
        /// GET /Groups/{groupId}/Invites
        /// </summary>
        [HttpGet("Groups/{groupId}/Invites")]
        [Authorize(Policy = "GroupModerator")]
        public async Task<IActionResult> ManageInvites(int groupId)
        {
            GroupAuthorizationResult moderatorCheck = await _groupAuthorization.EnsureModeratorAsync(groupId, User);
            if (!moderatorCheck.Succeeded)
            {
                return moderatorCheck.ToActionResult(this);
            }

            string? currentUserId = await _groupAuthorization.GetUserIdAsync(User);
            Group? group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }

            List<GroupInvite> invites = await _inviteService.GetInvitesForGroupAsync(groupId);

            var viewModel = new ManageInvitesViewModel
            {
                Group = GroupMapper.ToSummary(group),
                UserIsOwner = currentUserId != null && await _groupService.IsUserOwnerAsync(groupId, currentUserId),
                ModeratorsCanInvite = group.PrivacyLevel != PrivacyLevel.OwnerInvitePrivate,
                Invites = invites
                    .Select(i => new InviteListItemViewModel
                    {
                        Id = i.Id,
                        InviteCode = i.InviteCode,
                        CreatedBy = i.CreatedBy?.UserName ?? i.CreatedBy?.Email ?? i.CreatedById,
                        InvitedUser = i.InvitedUser != null ? (i.InvitedUser.UserName ?? i.InvitedUser.Email ?? i.InvitedUser.Id) : null,
                        CreatedAt = i.CreatedAt,
                        ExpiresAt = i.ExpiresAt,
                        OneTimeUse = i.OneTimeUse,
                        TimesUsed = i.TimesUsed,
                        IsExpired = i.IsExpired
                    })
                    .ToList()
            };

            return View(viewModel);
        }

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
            DateTime? expiresAtUtc = validation.ValidatedExpiresAt;

            GroupInvite newInvite = new GroupInvite
            {
                GroupId = invite.GroupId,
                CreatedById = userId,
                InvitedUserId = validation.InvitedUserId,
                InviteCode = inviteCode,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAtUtc,
                OneTimeUse = invite.OneTimeUse,
                TimesUsed = 0
            };

            newInvite = await _inviteService.CreateInviteAsync(newInvite);

            return RedirectToAction(nameof(ManageInvites), new { groupId = newInvite.GroupId });
        }

        /// <summary>
        /// Edit an existing invite.
        /// GET /Groups/{groupId}/Invites/{inviteId}/Edit
        /// </summary>
        [HttpGet("Groups/{groupId}/Invites/{inviteId}/Edit")]
        [Authorize(Policy = "GroupModerator")]
        public async Task<IActionResult> EditInvite(int groupId, int inviteId)
        {
            GroupAuthorizationResult moderatorCheck = await _groupAuthorization.EnsureModeratorAsync(groupId, User);
            if (!moderatorCheck.Succeeded)
            {
                return moderatorCheck.ToActionResult(this);
            }

            string? currentUserId = await _groupAuthorization.GetUserIdAsync(User);
            if (currentUserId == null)
            {
                return Challenge();
            }

            GroupInvite? invite = await _inviteService.GetInviteAsync(inviteId);
            if (invite == null || invite.GroupId != groupId)
            {
                return NotFound();
            }

            if (invite.Group.PrivacyLevel == PrivacyLevel.OwnerInvitePrivate && invite.Group.OwnerId != currentUserId)
            {
                return Forbid();
            }

            var viewModel = new GroupInviteEditViewModel
            {
                Id = invite.Id,
                GroupId = invite.GroupId,
                InviteCode = invite.InviteCode,
                OneTimeUse = invite.OneTimeUse,
                Expires = invite.ExpiresAt.HasValue,
                ExpiresAt = invite.ExpiresAt?.ToLocalTime(),
                InvitedUserDisplay = invite.InvitedUser != null ? (invite.InvitedUser.UserName ?? invite.InvitedUser.Email ?? invite.InvitedUser.Id) : null
            };

            return View(viewModel);
        }

        /// <summary>
        /// Saves changes to an invite.
        /// POST /Groups/{groupId}/Invites/{inviteId}/Edit
        /// </summary>
        [HttpPost("Groups/{groupId}/Invites/{inviteId}/Edit")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "GroupModerator")]
        public async Task<IActionResult> EditInvite(int groupId, int inviteId, GroupInviteEditViewModel model)
        {
            GroupAuthorizationResult moderatorCheck = await _groupAuthorization.EnsureModeratorAsync(groupId, User);
            if (!moderatorCheck.Succeeded)
            {
                return moderatorCheck.ToActionResult(this);
            }

            string? currentUserId = await _groupAuthorization.GetUserIdAsync(User);
            if (currentUserId == null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            GroupInvite? invite = await _inviteService.GetInviteAsync(inviteId);
            if (invite == null || invite.GroupId != groupId)
            {
                return NotFound();
            }

            InviteValidationResult validation = await _inviteService.ValidateUpdateInviteAsync(currentUserId, invite, model);
            if (validation.NotFound)
            {
                return NotFound();
            }
            if (validation.Unauthorized)
            {
                return Forbid();
            }
            if (!validation.IsValid)
            {
                foreach (KeyValuePair<string, string> kv in validation.FieldErrors)
                {
                    ModelState.AddModelError(kv.Key, kv.Value);
                }
                return View(model);
            }

            invite.ExpiresAt = validation.ValidatedExpiresAt;
            invite.OneTimeUse = model.OneTimeUse;

            await _inviteService.UpdateInviteAsync(invite);
            TempData["Success"] = "Invite updated.";

            return RedirectToAction(nameof(ManageInvites), new { groupId });
        }

        /// <summary>
        /// Deletes an invite to prevent further use.
        /// POST /Groups/{groupId}/Invites/{inviteId}/Delete
        /// </summary>
        [HttpPost("Groups/{groupId}/Invites/{inviteId}/Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "GroupModerator")]
        public async Task<IActionResult> DeleteInvite(int groupId, int inviteId)
        {
            GroupAuthorizationResult moderatorCheck = await _groupAuthorization.EnsureModeratorAsync(groupId, User);
            if (!moderatorCheck.Succeeded)
            {
                return moderatorCheck.ToActionResult(this);
            }

            string? currentUserId = await _groupAuthorization.GetUserIdAsync(User);
            if (currentUserId == null)
            {
                return Challenge();
            }

            GroupInvite? invite = await _inviteService.GetInviteAsync(inviteId);
            if (invite == null || invite.GroupId != groupId)
            {
                return NotFound();
            }

            if (invite.Group.PrivacyLevel == PrivacyLevel.OwnerInvitePrivate && invite.Group.OwnerId != currentUserId)
            {
                return Forbid();
            }

            await _inviteService.DeleteInviteAsync(inviteId);
            TempData["Success"] = "Invite deleted.";

            return RedirectToAction(nameof(ManageInvites), new { groupId });
        }

        #endregion
    }
}
