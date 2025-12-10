using CptcEvents.Application.Mappers;
using CptcEvents.Authorization;
using CptcEvents.Models;
using CptcEvents.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CptcEvents.Controllers
{
    /// <summary>
    /// Controller for managing group operations including creation, membership, and invitations.
    /// Provides functionality for group CRUD operations, member management, and group invites.
    /// Requires authentication for all actions except where explicitly marked otherwise.
    /// </summary>
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
        /// <summary>
        /// Displays groups for the authenticated user or redirects to a specific group's events.
        /// Admins see all groups, regular users see only groups they're members of.
        /// GET /Groups or /Groups/{groupId}
        /// </summary>
        /// <param name="groupId">Optional group ID. If provided, verifies membership and redirects to that group's events.</param>
        /// <returns>View with user's groups, or redirects to group events or Index if group not found or user not member.</returns>
        public async Task<IActionResult> Index(int? groupId)
        {
            string? userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            bool isAdmin = User.IsInRole("Admin");

            if (groupId != null)
            {
                Group? group = await _groupService.GetGroupByIdAsync(groupId.Value);
                if (group == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                // Admins can access any group, regular users need membership
                bool isMember = await _groupService.IsUserMemberAsync(groupId.Value, userId);
                if (!isAdmin && !isMember)
                {
                    return RedirectToAction(nameof(Index));
                }

                return RedirectToAction(nameof(Events), new { groupId = groupId.Value });
            }

            IEnumerable<Group> groups = await _groupService.GetGroupsForUserAsync(userId, isAdmin);

            return View(groups);
        }

        // GET: Groups/Create
        [HttpGet("Groups/Create")]
        /// <summary>
        /// Displays the group creation form.
        /// GET /Groups/Create
        /// </summary>
        /// <returns>Group creation form view.</returns>
        public IActionResult Create()
        {
            return View();
        }

        // POST: Groups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        /// <summary>
        /// Processes group creation with validation.
        /// POST /Groups/Create
        /// </summary>
        /// <param name="model">The group form data to create.</param>
        /// <returns>Redirects to Index on success, or form with validation errors on failure.</returns>
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
                Color = model.Color,
                OwnerId = userId
            };

            // Persist the new group to the database
            await _groupService.CreateGroupAsync(newGroup);

            return RedirectToAction(nameof(Index));
        }

        // GET: Groups/Edit/5
        [HttpGet("Groups/Edit/{groupId}")]
        [Authorize(Policy = "GroupOwner")]
        /// <summary>
        /// Displays the group editing form for group owners.
        /// GET /Groups/Edit/{groupId}
        /// </summary>
        /// <param name="groupId">The ID of the group to edit.</param>
        /// <returns>Group editing form view, or redirects to Index if group not found or user not owner.</returns>
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
                return RedirectToAction(nameof(Index));
            }

            bool isAdmin = User.IsInRole("Admin");
            bool isOwner = isAdmin || await _groupService.IsUserOwnerAsync(groupId, userId);

            var viewModel = new GroupFormViewModel
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                PrivacyLevel = group.PrivacyLevel,
                Color = group.Color,
                IsOwner = isOwner
            };

            return View(viewModel);
        }

        // POST: Groups/Edit/5
        [HttpPost("Groups/Edit/{groupId}")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "GroupOwner")]
        /// <summary>
        /// Processes group updates with authorization and validation.
        /// POST /Groups/Edit/{groupId}
        /// </summary>
        /// <param name="groupId">The ID of the group to update.</param>
        /// <param name="model">The updated group form data.</param>
        /// <returns>Redirects to ManageGroup on success, or form with validation errors on failure.</returns>
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
                bool isAdmin = User.IsInRole("Admin");
                model.IsOwner = isAdmin || await _groupService.IsUserOwnerAsync(groupId, userId);
                return View(model);
            }

            Group? existingGroup = await _groupService.GetGroupByIdAsync(groupId);
            if (existingGroup == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Update group properties
            existingGroup.Name = model.Name;
            existingGroup.Description = model.Description;
            existingGroup.PrivacyLevel = model.PrivacyLevel;
            existingGroup.Color = model.Color;

            Group? result = await _groupService.UpdateGroupAsync(existingGroup, userId);
            if (result == null)
            {
                ModelState.AddModelError(string.Empty, "Failed to update group. You may not have permission to make these changes.");
                bool isAdmin = User.IsInRole("Admin");
                model.IsOwner = isAdmin || await _groupService.IsUserOwnerAsync(groupId, userId);
                return View(model);
            }

            return RedirectToAction(nameof(ManageGroup), new { groupId = groupId });
        }

        // GET: Groups/Delete/5
        [HttpGet("Groups/Delete/{groupId}")]
        [Authorize(Policy = "GroupOwner")]
        /// <summary>
        /// Displays the group deletion confirmation page.
        /// GET /Groups/Delete/{groupId}
        /// </summary>
        /// <param name="groupId">The ID of the group to delete.</param>
        /// <returns>Deletion confirmation view, or redirects to Index if group not found.</returns>
        public async Task<IActionResult> Delete(int groupId)
        {
            Group? group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(group);
        }

        // POST: Groups/Delete/5
        [HttpPost("Groups/Delete/{groupId}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "GroupOwner")]
        /// <summary>
        /// Processes group deletion with authorization checks.
        /// POST /Groups/Delete/{groupId}
        /// </summary>
        /// <param name="groupId">The ID of the group to delete.</param>
        /// <returns>Redirects to Index on success, or to Index if group not found.</returns>
        public async Task<IActionResult> DeleteConfirmed(int groupId)
        {
            Group? group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                return RedirectToAction(nameof(Index));
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
                return RedirectToAction(nameof(Index));
            }

            bool isAdmin = User.IsInRole("Admin");
            bool isOwner = isAdmin || (userId != null && await _groupService.IsUserOwnerAsync(groupId, userId));
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
        /// <summary>
        /// Displays the group join page for public groups.
        /// GET /Groups/Join or /Groups/Join/{groupId}
        /// </summary>
        /// <param name="groupId">Optional group ID to join. If provided, displays join confirmation for that group.</param>
        /// <returns>Join form view, or redirects if group not found, user already member, or group not public.</returns>
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
                return RedirectToAction(nameof(Join));
            }

            bool isMember = await _groupService.IsUserMemberAsync(groupId.Value, userId);
            if (isMember)
            {
                TempData["Info"] = "You are already a member of this group.";
                return RedirectToAction(nameof(Events), new { groupId = groupId });
            }

            if (group.PrivacyLevel != PrivacyLevel.Public)
            {
                return RedirectToAction(nameof(Join));
            }

            return View(group);
        }

        // POST: Groups/Join
        [HttpPost("Groups/Join/{groupId}")]
        [ValidateAntiForgeryToken]
        [ActionName("Join")]
        /// <summary>
        /// Processes the user joining a public group.
        /// POST /Groups/Join/{groupId}
        /// </summary>
        /// <param name="groupId">The ID of the group to join.</param>
        /// <returns>Redirects to group events on success, or redirects to Join if unable to join.</returns>
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
                return RedirectToAction(nameof(Join));
            }

            bool isMember = await _groupService.IsUserMemberAsync(groupId, userId);
            if (isMember)
            {
                TempData["Info"] = "You are already a member of this group.";
                return RedirectToAction(nameof(Events), new { groupId = groupId });
            }

            if (group.PrivacyLevel != PrivacyLevel.Public)
            {
                return RedirectToAction(nameof(Join));
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
        /// <summary>
        /// Displays the group leave confirmation page.
        /// GET /Groups/Leave/{groupId}
        /// </summary>
        /// <param name="groupId">The ID of the group to leave.</param>
        /// <returns>Leave confirmation view, or redirects to Index if group not found or user not member.</returns>
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
                return RedirectToAction(nameof(Index));
            }

            bool isAdmin = User.IsInRole("Admin");
            bool isOwner = isAdmin || await _groupService.IsUserOwnerAsync(groupId, userId);
            ViewBag.IsOwner = isOwner;

            return View(group);
        }

        // POST: Groups/Leave/5
        [HttpPost("Groups/Leave/{groupId}")]
        [ActionName("Leave")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "GroupMember")]
        /// <summary>
        /// Processes the user leaving a group.
        /// POST /Groups/Leave/{groupId}
        /// </summary>
        /// <param name="groupId">The ID of the group to leave.</param>
        /// <returns>Redirects to Index on success, or shows error if user is group owner.</returns>
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
                return RedirectToAction(nameof(Index));
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
        /// <summary>
        /// Displays the group member management page for group owners.
        /// GET /Groups/{groupId}/Members
        /// </summary>
        /// <param name="groupId">The ID of the group whose members to manage.</param>
        /// <returns>Member management view, or redirects to Index if group not found or user not owner.</returns>
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
                return RedirectToAction(nameof(Index));
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
                ModeratorsCanInvite = group.PrivacyLevel != PrivacyLevel.OwnerInvitePrivate,
                Members = members
            };

            return View(viewModel);
        }

        // POST: Groups/{groupId}/Members/{userId}/Role
        [HttpPost("Groups/{groupId}/Members/{userId}/Role")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "GroupOwner")]
        /// <summary>
        /// Updates a group member's role (promote/demote between Member and Moderator).
        /// POST /Groups/{groupId}/Members/{userId}/Role
        /// </summary>
        /// <param name="groupId">The ID of the group.</param>
        /// <param name="userId">The ID of the user whose role to update.</param>
        /// <param name="newRole">The new role for the user.</param>
        /// <returns>Redirects to ManageMembers with status message.</returns>
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
        /// <summary>
        /// Removes a member from the group.
        /// POST /Groups/{groupId}/Members/{userId}/Remove
        /// </summary>
        /// <param name="groupId">The ID of the group.</param>
        /// <param name="userId">The ID of the user to remove.</param>
        /// <returns>Redirects to ManageMembers with status message.</returns>
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
                return RedirectToAction(nameof(Index));
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
        /// <summary>
        /// Displays upcoming events for a specific group.
        /// GET /Groups/{groupId}/Events
        /// </summary>
        /// <param name="groupId">The ID of the group whose events to display.</param>
        /// <returns>View with upcoming events for the group, or redirects if group not found or user not member.</returns>
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
                return RedirectToAction(nameof(Index));
            }

            string? userId = await _groupAuthorization.GetUserIdAsync(User);

            bool isAdmin = User.IsInRole("Admin");
            bool isModerator =
                isAdmin
                || (userId != null && await _groupService.IsUserModeratorAsync(groupId, userId))
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
        /// <summary>
        /// Displays the event management page for group moderators and owners.
        /// GET /Groups/{groupId}/ManageEvents
        /// </summary>
        /// <param name="groupId">The ID of the group whose events to manage.</param>
        /// <returns>Event management view, or redirects if group not found or user not moderator.</returns>
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
                return RedirectToAction(nameof(Index));
            }

            IEnumerable<Event> events = await _eventService.GetEventsForGroupAsync(groupId);
            string? user = await _groupAuthorization.GetUserIdAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            bool isAdmin = User.IsInRole("Admin");
            bool isOwner = isAdmin || await _groupService.IsUserOwnerAsync(groupId, user);

            var viewModel = new GroupEventsViewModel
            {
                Group = GroupMapper.ToSummary(group),
                UserIsModerator = true,
                UserIsOwner = isOwner,
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
        /// <param name="groupId">The ID of the group whose invites to manage.</param>
        /// <returns>Invite management view, or redirects if group not found or user not moderator.</returns>
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
                return RedirectToAction(nameof(Index));
            }

            List<GroupInvite> invites = await _inviteService.GetInvitesForGroupAsync(groupId);

            bool isAdmin = User.IsInRole("Admin");
            bool isOwner = isAdmin || (currentUserId != null && await _groupService.IsUserOwnerAsync(groupId, currentUserId));

            var viewModel = new ManageInvitesViewModel
            {
                Group = GroupMapper.ToSummary(group),
                UserIsOwner = isOwner,
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
                return Challenge();
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
                return Challenge();
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
                return RedirectToAction(nameof(Index));
            }
            if (validation.Unauthorized)
            {
                return Challenge();
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
        /// <summary>
        /// Edit an existing invite.
        /// GET /Groups/{groupId}/Invites/{inviteId}/Edit
        /// </summary>
        /// <param name="groupId">The ID of the group.</param>
        /// <param name="inviteId">The ID of the invite to edit.</param>
        /// <returns>Invite editing form view, or redirects if invite not found or user lacks authorization.</returns>
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
                return RedirectToAction(nameof(Index));
            }

            if (invite.Group.PrivacyLevel == PrivacyLevel.OwnerInvitePrivate && invite.Group.OwnerId != currentUserId)
            {
                return RedirectToAction(nameof(Index));
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
        /// <param name="groupId">The ID of the group.</param>
        /// <param name="inviteId">The ID of the invite to update.</param>
        /// <param name="model">The updated invite form data.</param>
        /// <returns>Redirects to ManageInvites on success, or form with validation errors on failure.</returns>
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
                return RedirectToAction(nameof(Index));
            }

            InviteValidationResult validation = await _inviteService.ValidateUpdateInviteAsync(currentUserId, invite, model);
            if (validation.NotFound)
            {
                return RedirectToAction(nameof(Index));
            }
            if (validation.Unauthorized)
            {
                return Challenge();
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
        /// <param name="groupId">The ID of the group.</param>
        /// <param name="inviteId">The ID of the invite to delete.</param>
        /// <returns>Redirects to ManageInvites on success or if invite not found.</returns>
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
                return RedirectToAction(nameof(Index));
            }

            if (invite.Group.PrivacyLevel == PrivacyLevel.OwnerInvitePrivate && invite.Group.OwnerId != currentUserId)
            {
                return RedirectToAction(nameof(Index));
            }

            await _inviteService.DeleteInviteAsync(inviteId);
            TempData["Success"] = "Invite deleted.";

            return RedirectToAction(nameof(ManageInvites), new { groupId });
        }

        #endregion
    }
}
