using CptcEvents.Application.Mappers;
using CptcEvents.Authorization;
using CptcEvents.Data;
using CptcEvents.Models;
using CptcEvents.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CptcEvents.Controllers
{
    /// <summary>
    /// Controller for managing event operations including CRUD operations and calendar views.
    /// Requires authentication for all actions.
    /// </summary>
    public class EventsController : Controller
    {
        private readonly IEventService _eventsService;
        private readonly IGroupService _groupService;
        private readonly IGroupAuthorizationService _groupAuthorization;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventsController(IEventService eventsService, IGroupService groupService, IGroupAuthorizationService groupAuthorization, UserManager<ApplicationUser> userManager)
        {
            _eventsService = eventsService;
            _groupService = groupService;
            _groupAuthorization = groupAuthorization;
            _userManager = userManager;
        }

        #region Event CRUD Operations

        /// <summary>
        /// Displays a list of upcoming events for the authenticated user.
        /// Admins see all events, regular users see only events from their groups.
        /// GET /Events or /Events?eventId={eventId}
        /// </summary>
        /// <param name="eventId">Optional event ID parameter (not currently used).</param>
        /// <returns>View with list of active events for the user.</returns>
        [Authorize]
        public async Task<IActionResult> Index(int? eventId)
        {
            string? userId = await _groupAuthorization.GetUserIdAsync(User);
            if (userId == null)
            {
                return Challenge();
            }

            bool isAdmin = User.IsInRole("Admin");
            IEnumerable<Event> events = await _eventsService.GetActiveEventsForUserAsync(userId, isAdmin);

            List<EventDetailsViewModel> viewModel = events.Select(EventMapper.ToDetails).ToList();

            return View(viewModel);
        }

        // GET: Events/Create
        [HttpGet("Events/Create")]
        [Authorize]
        /// <summary>
        /// Displays the event creation form, optionally filtered to a specific group.
        /// GET /Events/Create?groupId={groupId}
        /// </summary>
        /// <param name="groupId">Optional group ID to pre-select when creating an event.</param>
        /// <returns>Event creation form view, or redirects if user lacks permissions.</returns>
        public async Task<IActionResult> Create([FromQuery] int? groupId)
        {
            // Ensure user is authenticated
            string? userId = await _groupAuthorization.GetUserIdAsync(User);
            if (userId == null)
            {
                return Challenge();
            }

            // Load groups for the current user
            await PopulateGroupsSelectListAsync(groupId);

            // Check if user has any groups where they can create events
            var selectList = ViewData["Groups"] as SelectList;
            if (selectList == null || selectList.Count() == 0)
            {
                TempData["Error"] = "You must be a moderator in at least one group to create events. Please create or join a group first.";
                return RedirectToAction(nameof(GroupsController.Create), "Groups");
            }

            if (groupId.HasValue)
            {
                GroupAuthorizationResult moderatorCheck = await _groupAuthorization.EnsureModeratorAsync(groupId.Value, User);
                if (!moderatorCheck.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View();
        }

        // POST: Events/Create
        [HttpPost("Events/Create")]
        [ValidateAntiForgeryToken]
        [Authorize]
        /// <summary>
        /// Processes event creation with validation and authorization checks.
        /// POST /Events/Create
        /// </summary>
        /// <param name="model">The event form data to create.</param>
        /// <returns>Redirects to ManageEvents on success, or form with validation errors on failure.</returns>
        public async Task<IActionResult> Create(EventFormViewModel model)
        {
            GroupAuthorizationResult moderatorCheck = await _groupAuthorization.EnsureModeratorAsync(model.GroupId, User);
            if (!moderatorCheck.Succeeded)
            {
                return moderatorCheck.ToActionResult(this);
            }

            // Only Staff and Admin roles can make events public
            if (model.IsPublic && !User.IsInRole("Staff") && !User.IsInRole("Admin"))
            {
                ModelState.AddModelError(string.Empty, "Only staff members can create public events.");
                await PopulateGroupsSelectListAsync();
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                await PopulateGroupsSelectListAsync();
                return View(model);
            }

            Event newEvent = EventMapper.ToEntity(model);
            
            // Set the user who created the event
            string? userId = await _groupAuthorization.GetUserIdAsync(User);
            if (userId == null)
            {
                return Challenge();
            }
            newEvent.CreatedByUserId = userId;

            Event created = await _eventsService.CreateEventAsync(newEvent);

            return RedirectToAction(nameof(GroupsController.ManageEvents), "Groups", new { groupId = created.GroupId });
        }

        // GET: Events/Edit/5
        [HttpGet("Events/Edit/{eventId}")]
        [Authorize]
        /// <summary>
        /// Displays the event editing form for a specific event.
        /// GET /Events/Edit/{eventId}
        /// </summary>
        /// <param name="eventId">The ID of the event to edit.</param>
        /// <returns>Event editing form view, or redirects if event not found or user lacks permissions.</returns>
        public async Task<IActionResult> Edit(int eventId)
        {
            Event? eventItem = await _eventsService.GetEventByIdAsync(eventId);
            if (eventItem == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Prevent students from editing staff/admin events
            var user = await _userManager.GetUserAsync(User);
            if (User.IsInRole("Student") && !await CanStudentEditEventAsync(eventItem, user))
            {
                return Forbid();
            }

            GroupAuthorizationResult moderatorCheck = await _groupAuthorization.EnsureModeratorAsync(eventItem.GroupId, User);
            if (!moderatorCheck.Succeeded)
            {
                return moderatorCheck.ToActionResult(this);
            }

            var viewModel = new EventFormViewModel
            {
                Id = eventItem.Id,
                Title = eventItem.Title,
                Description = eventItem.Description,
                GroupId = eventItem.GroupId,
                GroupName = eventItem.Group?.Name,
                IsPublic = eventItem.IsPublic,
                IsApprovedPublic = eventItem.IsApprovedPublic,
                IsDeniedPublic = eventItem.IsDeniedPublic,
                IsAllDay = eventItem.IsAllDay,
                DateOfEvent = eventItem.DateOfEvent,
                StartTime = eventItem.StartTime,
                EndTime = eventItem.EndTime,
                Url = eventItem.Url,
                IsModerator = true
            };

            return View(viewModel);
        }

        // POST: Events/Edit/5
        [HttpPost("Events/Edit/{eventId}")]
        [ValidateAntiForgeryToken]
        [Authorize]
        /// <summary>
        /// Processes event updates with validation and authorization checks.
        /// POST /Events/Edit/{eventId}
        /// </summary>
        /// <param name="eventId">The ID of the event to update.</param>
        /// <param name="model">The updated event form data.</param>
        /// <returns>Redirects to ManageEvents on success, or form with validation errors on failure.</returns>
        public async Task<IActionResult> Edit(int eventId, EventFormViewModel model)
        {
            if (eventId != model.Id)
            {
                return BadRequest();
            }

            // Retrieve existing event
            Event? existingEvent = await _eventsService.GetEventByIdAsync(eventId);
            if (existingEvent == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Prevent students from editing staff/admin events
            var user = await _userManager.GetUserAsync(User);
            if (User.IsInRole("Student") && !await CanStudentEditEventAsync(existingEvent, user))
            {
                return Forbid();
            }

            // Prevent student created events from being made public
            var eventCreator = await _userManager.FindByIdAsync(existingEvent.CreatedByUserId);
            if (eventCreator != null && await _userManager.IsInRoleAsync(eventCreator, "Student") && model.IsPublic)
            {
                ModelState.AddModelError(string.Empty, "Events created by students cannot be made public.");
                model.IsModerator = true;
                model.GroupName = existingEvent.Group?.Name;
                return View(model);
            }

            GroupAuthorizationResult moderatorCheck = await _groupAuthorization.EnsureModeratorAsync(existingEvent.GroupId, User);
            if (!moderatorCheck.Succeeded)
            {
                return moderatorCheck.ToActionResult(this);
            }

            // Only Staff and Admin roles can make events public
            if (model.IsPublic && !User.IsInRole("Staff") && !User.IsInRole("Admin"))
            {
                ModelState.AddModelError(string.Empty, "Only staff members can create public events.");
                model.IsModerator = true;
                model.GroupName = existingEvent.Group?.Name;
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                model.IsModerator = true;
                model.GroupName = existingEvent.Group?.Name;
                return View(model);
            }

            var updatedEvent = new Event();
            EventMapper.ApplyUpdates(model, updatedEvent);

            Event? result = await _eventsService.UpdateEventAsync(existingEvent.Id, updatedEvent);
            if (result == null)
            {
                ModelState.AddModelError(string.Empty, "Failed to update event.");
                model.IsModerator = true;
                return View(model);
            }

            return RedirectToAction(nameof(GroupsController.ManageEvents), "Groups", new { groupId = result.GroupId });
        }

        // GET: Events/Delete/5
        [HttpGet("Events/Delete/{eventId}")]
        [Authorize]
        /// <summary>
        /// Displays the event deletion confirmation page.
        /// GET /Events/Delete/{eventId}
        /// </summary>
        /// <param name="eventId">The ID of the event to delete.</param>
        /// <returns>Deletion confirmation view, or redirects if event not found or user lacks permissions.</returns>
        public async Task<IActionResult> Delete(int eventId)
        {
            Event? eventItem = await _eventsService.GetEventByIdAsync(eventId);
            if (eventItem == null)
            {
                return RedirectToAction(nameof(Index));
            }

            GroupAuthorizationResult moderatorCheck = await _groupAuthorization.EnsureModeratorAsync(eventItem.GroupId, User);
            if (!moderatorCheck.Succeeded)
            {
                return moderatorCheck.ToActionResult(this);
            }

            return View(EventMapper.ToDetails(eventItem));
        }

        // POST: Events/Delete/5
        [HttpPost("Events/Delete/{eventId}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        /// <summary>
        /// Processes event deletion with authorization checks.
        /// POST /Events/Delete/{eventId}
        /// </summary>
        /// <param name="eventId">The ID of the event to delete.</param>
        /// <returns>Redirects to ManageEvents on success, or to Index if event not found.</returns>
        public async Task<IActionResult> DeleteConfirmed(int eventId)
        {
            Event? eventItem = await _eventsService.GetEventByIdAsync(eventId);
            if (eventItem == null)
            {
                return RedirectToAction(nameof(Index));
            }

            GroupAuthorizationResult moderatorCheck = await _groupAuthorization.EnsureModeratorAsync(eventItem.GroupId, User);
            if (!moderatorCheck.Succeeded)
            {
                return moderatorCheck.ToActionResult(this);
            }

            await _eventsService.DeleteEventAsync(eventId);

            return RedirectToAction(nameof(GroupsController.ManageEvents), "Groups", new { groupId = eventItem.GroupId });
        }

        // GET: Events/Details/5
        [HttpGet("Events/Details/{eventId}")]
        /// <summary>
        /// Returns JSON data for a single event to display in a modal.
        /// GET /Events/Details/{eventId}
        /// </summary>
        /// <param name="eventId">The ID of the event to retrieve.</param>
        /// <returns>JSON with event details or NotFound if event doesn't exist or user lacks access.</returns>
        public async Task<IActionResult> Details(int eventId)
        {
            Event? eventItem = await _eventsService.GetEventByIdAsync(eventId);
            if (eventItem == null)
            {
                return NotFound();
            }

            // Check if user is member of the group (needed for both access control and UI display)
            bool isAdmin = false;
            bool isUserMember = false;

            string? userId = await _groupAuthorization.GetUserIdAsync(User);
            if (userId != null)
            {
                isAdmin = User.IsInRole("Admin");
                isUserMember = await _groupService.IsUserMemberAsync(eventItem.GroupId, userId);
            }
            
            // If event is not public, check if user is member of the group (admins can access all events)
            if (!eventItem.IsPublic || !eventItem.IsApprovedPublic)
            {
                if (!isAdmin && !isUserMember)
                {
                    return Forbid();
                }
            }

            // For UI purposes, treat admins as members (so they can see the "View Group" button)
            bool canAccessGroup = isAdmin || isUserMember;
            var eventDetails = EventMapper.ToDetails(eventItem, canAccessGroup);
            return Json(eventDetails);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Determines if a student user can edit the given event.
        /// Students can only edit events they created or events created by other students.
        /// </summary>
        [Authorize]
        private async Task<bool> CanStudentEditEventAsync(Event eventItem, ApplicationUser? currentUser)
        {
            if (currentUser == null)
                return false;

            // Student can edit if they created it
            if (eventItem.CreatedByUserId == currentUser.Id)
                return true;

            // Student cannot edit if staff/admin created it
            var eventCreator = await _userManager.FindByIdAsync(eventItem.CreatedByUserId);
            if (eventCreator != null && (await _userManager.IsInRoleAsync(eventCreator, "Staff") || await _userManager.IsInRoleAsync(eventCreator, "Admin")))
                return false;

            // Student can edit if another student created it
            return true;
        }

        // Helper that populates ViewData["Groups"] with groups available to the current user.
        [Authorize]
        private async Task PopulateGroupsSelectListAsync(int? selectedGroupId = null)
        {
            var groups = new List<Group>();
            if (User?.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    // Only get groups where user is at least a moderator (can create events)
                    var allGroups = await _groupService.GetGroupsForUserAsync(user.Id);
                    foreach (var group in allGroups)
                    {
                        bool isModerator = await _groupService.IsUserModeratorAsync(group.Id, user.Id);
                        if (isModerator)
                        {
                            groups.Add(group);
                        }
                    }
                }
            }

            ViewData["Groups"] = new SelectList(groups, "Id", "Name", selectedGroupId);
        }

        #endregion

        #region API Endpoints

        /// <summary>
        /// Returns all events for a specific group formatted for FullCalendar.
        /// Ensures the requesting user is authenticated and a member of the group.
        /// GET /Events/Group/{groupId}/Events
        /// </summary>
        /// <param name="groupId">The group whose events should be returned.</param>
        /// <returns>JSON array of FullCalendar-compatible event objects.</returns>
        [HttpGet("Events/Group/{groupId}/Events")]
        [Authorize]
        public async Task<IActionResult> GetGroupEvents(int groupId)
        {
            string? userId = User?.Identity?.IsAuthenticated == true ? _userManager.GetUserId(User) : null;
            if (userId == null)
            {
                return Challenge();
            }

            Group? group = await _groupService.GetGroupByIdAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }

            bool isAdmin = User?.IsInRole("Admin") == true;
            bool isMember = await _groupService.IsUserMemberAsync(groupId, userId);
            
            // Admins can access any group, regular users need membership
            if (!isAdmin && !isMember)
            {
                return Forbid();
            }

            IEnumerable<Event> events = await _eventsService.GetEventsForGroupAsync(groupId);
            var fullCalendarEvents = events.Select(EventMapper.ToFullCalendarEvent).ToList();

            return Json(fullCalendarEvents);
        }

        /// <summary>
        /// Retrieves all approved public calendar events and returns their data formatted for FullCalendar.
        /// Logged-in users also see events from their groups. Admins see all events.
        /// GET /Events/GetEvents
        /// </summary>
        /// <remarks>This method is intended for use by pages that require event data
        /// formatted for the FullCalendar JavaScript library. The returned list will be empty if no events are
        /// found. Only events marked as both IsPublic and IsApprovedPublic are included for non-admin users.</remarks>
        /// <returns>A JSON result containing a list of event objects formatted for FullCalendar.</returns>
        public async Task<IActionResult> GetEvents()
        {
            var user = await _userManager.GetUserAsync(User);
            IEnumerable<Event> events;

            if (user != null)
            {
                bool isAdmin = User.IsInRole("Admin");
                
                if (isAdmin)
                {
                    // Admins see all events
                    events = await _eventsService.GetEventsForUserAsync(user.Id, isAdmin);
                }
                else
                {
                    // Regular logged-in users see approved public events + events from their groups
                    var approvedPublicEvents = await _eventsService.GetApprovedPublicEventsAsync();
                    var userGroupEvents = await _eventsService.GetEventsForUserAsync(user.Id);
                    events = approvedPublicEvents.Concat(userGroupEvents).DistinctBy(e => e.Id);
                }
            }
            else
            {
                // Anonymous users only see approved public events
                events = await _eventsService.GetApprovedPublicEventsAsync();
            }

            var fullCalendarEvents = events
                .Select(EventMapper.ToFullCalendarEvent)
                .ToList();

            return Json(fullCalendarEvents);
        }

        /// <summary>
        /// Returns events within an inclusive date range formatted for FullCalendar.
        /// GET /Events/GetEventsInRange?start=yyyy-MM-dd&end=yyyy-MM-dd
        /// </summary>
        /// <param name="start">The start date (inclusive) in yyyy-MM-dd format.</param>
        /// <param name="end">The end date (inclusive) in yyyy-MM-dd format.</param>
        /// <returns>JSON array of FullCalendar-compatible event objects within the date range.</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetEventsInRange([FromQuery] DateOnly start, [FromQuery] DateOnly end)
        {
            if (end < start)
            {
                return BadRequest("End date must be on or after start date.");
            }

            IEnumerable<Event> events = await _eventsService.GetEventsInRangeAsync(start, end);
            var fullCalendarEvents = events.Select(EventMapper.ToFullCalendarEvent).ToList();
            return Json(fullCalendarEvents);
        }

        #endregion
    }
}
