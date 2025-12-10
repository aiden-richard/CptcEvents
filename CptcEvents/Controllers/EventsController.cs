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
    [Authorize]
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
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index(int? eventId)
        {
            string? userId = await _groupAuthorization.GetUserIdAsync(User);
            if (userId == null)
            {
                return Challenge();
            }

            IEnumerable<Event> events = await _eventsService.GetActiveEventsForUserAsync(userId);

            List<EventDetailsViewModel> viewModel = events.Select(EventMapper.ToDetails).ToList();

            return View(viewModel);
        }

        // GET: Events/Create
        [HttpGet("Events/Create")]
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
                    return NotFound();
                }
            }
            return View();
        }

        // POST: Events/Create
        [HttpPost("Events/Create")]
        [ValidateAntiForgeryToken]
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

            Event created = await _eventsService.CreateEventAsync(newEvent);

            return RedirectToAction(nameof(GroupsController.ManageEvents), "Groups", new { groupId = created.GroupId });
        }

        // GET: Events/Edit/5
        [HttpGet("Events/Edit/{eventId}")]
        public async Task<IActionResult> Edit(int eventId)
        {
            Event? eventItem = await _eventsService.GetEventByIdAsync(eventId);
            if (eventItem == null)
            {
                return NotFound();
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
                return NotFound();
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
        public async Task<IActionResult> Delete(int eventId)
        {
            Event? eventItem = await _eventsService.GetEventByIdAsync(eventId);
            if (eventItem == null)
            {
                return NotFound();
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
        public async Task<IActionResult> DeleteConfirmed(int eventId)
        {
            Event? eventItem = await _eventsService.GetEventByIdAsync(eventId);
            if (eventItem == null)
            {
                return NotFound();
            }

            GroupAuthorizationResult moderatorCheck = await _groupAuthorization.EnsureModeratorAsync(eventItem.GroupId, User);
            if (!moderatorCheck.Succeeded)
            {
                return moderatorCheck.ToActionResult(this);
            }

            await _eventsService.DeleteEventAsync(eventId);

            return RedirectToAction(nameof(GroupsController.ManageEvents), "Groups", new { groupId = eventItem.GroupId });
        }

        #endregion

        #region Helper Methods

        // Helper that populates ViewData["Groups"] with groups available to the current user.
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
        /// </summary>
        /// <param name="groupId">The group whose events should be returned.</param>
        /// <returns>JSON array of FullCalendar-compatible event objects.</returns>
        [HttpGet("Events/Group/{groupId}/Events")]
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

            bool isMember = await _groupService.IsUserMemberAsync(groupId, userId);
            if (!isMember)
            {
                return Forbid();
            }

            IEnumerable<Event> events = await _eventsService.GetEventsForGroupAsync(groupId);
            var fullCalendarEvents = events.Select(EventMapper.ToFullCalendarEvent).ToList();

            return Json(fullCalendarEvents);
        }

        /// <summary>
        /// Retrieves all public calendar events and returns their data formatted for FullCalendar.
        /// </summary>
        /// <remarks>This method is intended for use by pages that require event data
        /// formatted for the FullCalendar JavaScript library. The returned list will be empty if no events are
        /// found.</remarks>
        /// <returns>A JSON result containing a list of event objects formatted for FullCalendar.</returns>
        [AllowAnonymous]
        public async Task<IActionResult> GetEvents()
        {
            var user = await _userManager.GetUserAsync(User);
            IEnumerable<Event> events;

            if (user != null)
            {
                // Logged-in users see public events + events from their groups
                var publicEvents = await _eventsService.GetPublicEventsAsync();
                var userGroupEvents = await _eventsService.GetEventsForUserAsync(user.Id);
                events = publicEvents.Union(userGroupEvents).Distinct();
            }
            else
            {
                // Anonymous users only see public events
                events = await _eventsService.GetPublicEventsAsync();
            }

            var fullCalendarEvents = events
                .Select(EventMapper.ToFullCalendarEvent)
                .ToList();

            return Json(fullCalendarEvents);
        }

        /// <summary>
        /// Returns events within an inclusive date range formatted for FullCalendar.
        /// Query parameters: start=yyyy-MM-dd, end=yyyy-MM-dd
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
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
